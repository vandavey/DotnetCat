using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using DotnetCat.Pipelines;
using ArgNullException = System.ArgumentNullException;
using Env = System.Environment;
using Cmd = DotnetCat.Handlers.CommandHandler;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Base class for all TCP socket nodes in DotnetCat.Nodes
    /// </summary>
    class Node : ISockErrorHandled
    {
        private List<StreamPipe> _pipes;

        private Process _process;

        private StreamReader _netReader;
        private StreamWriter _netWriter;

        /// <summary>
        /// Initialize object
        /// </summary>
        protected Node()
        {
            Port = 4444;
            Verbose = false;
            Client = new TcpClient();
        }

        /// <summary>
        /// Initialize object
        /// </summary>
        protected Node(IPAddress address) : this()
        {
            Addr = address;
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        ~Node() => Dispose();

        public bool Verbose { get; set; }

        public int Port { get; set; }

        public string FilePath { get; set; }

        public string Exe { get; set; }

        public IPAddress Addr { get; set; }

        public TcpClient Client { get; set; }

        protected static bool Transfer => Program.Transfer != TransferOpt.None;

        protected static Platform OS => Program.OS;

        protected bool UsingExe => Exe is not null;

        protected NetworkStream NetStream { get; set; }

        /// <summary>
        /// Initialize and run an executable process
        /// </summary>
        public bool Start(string exe = null)
        {
            Exe ??= Cmd.GetDefaultExe(OS);

            // Invalid executable path
            if (!Cmd.ExistsOnPath(exe).exists)
            {
                Dispose();
                ErrorHandler.Handle(Except.ExePath, exe, true);
            }

            _process = new Process
            {
                StartInfo = GetStartInfo(exe)
            };
            return _process.Start();
        }

        /// <summary>
        /// Get ProcessStartInfo to use for executable startup
        /// </summary>
        public static ProcessStartInfo GetStartInfo(string shell)
        {
            _ = shell ?? throw new ArgNullException(nameof(shell));

            // Exe process startup information
            ProcessStartInfo info = new(shell)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,

                // Load user profile path
                WorkingDirectory = OS switch
                {
                    Platform.Nix => Env.GetEnvironmentVariable("HOME"),
                    Platform.Win => Env.GetEnvironmentVariable("USERPROFILE"),
                    _ => Env.CurrentDirectory
                }
            };

            // Profile loading only supported on Windows
            if (OperatingSystem.IsWindows())
            {
                info.LoadUserProfile = true;
            }
            return info;
        }

        /// <summary>
        /// Activate communication between pipe streams
        /// </summary>
        public virtual void Connect()
        {
            _ = NetStream ?? throw new ArgNullException(nameof(NetStream));

            // Invalid argument combination
            if (UsingExe && Transfer)
            {
                string message = "--exec, --output/--send";
                PipeError(Except.ArgsCombo, message);
            }

            AddPipes(Program.PipeVariant);
            _pipes?.ForEach(pipe => pipe?.Connect());
        }

        /// <summary>
        /// Dispose of unmanaged socket resources and handle error
        /// </summary>
        public virtual void PipeError(Except type, IPEndPoint ep,
                                                   Exception ex = null,
                                                   Level level = Level.Error) {
            PipeError(type, ep.ToString(), ex, level);
        }

        /// <summary>
        /// Dispose of unmanaged resources and handle error
        /// </summary>
        public virtual void PipeError(Except type, string arg,
                                                   Exception ex = null,
                                                   Level level = Level.Error) {
            Dispose();
            ErrorHandler.Handle(type, arg, ex, level);
        }

        /// <summary>
        /// Release any unmanaged resources
        /// </summary>
        public virtual void Dispose()
        {
            _pipes?.ForEach(pipe => pipe?.Dispose());

            _process?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            Client?.Dispose();
            NetStream?.Dispose();

            // Prevent unnecessary finalization
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize socket stream pipelines
        /// </summary>
        protected void AddPipes(PipeType pipeType)
        {
            _ = NetStream ?? throw new(nameof(NetStream));

            // Can't perform socket read/write operations
            if (!NetStream.CanRead || !NetStream.CanWrite)
            {
                string msg = "Can't perform stream read/write operations";
                throw new ArgumentException(nameof(NetStream), msg);
            }

            _netReader = new StreamReader(NetStream);
            _netWriter = new StreamWriter(NetStream) { AutoFlush = true };

            // Initialize socket pipeline(s)
            _pipes = pipeType switch
            {
                PipeType.File => GetTransferPipes(),
                PipeType.Process => GetProcessPipes(),
                PipeType.Text => GetTextPipes(),
                _ => GetDefaultPipes()
            };
        }

        /// <summary>
        /// Wait for pipeline(s) to be disconnected
        /// </summary>
        protected void WaitForExit(int msDelay = 100)
        {
            while (Client.Connected)
            {
                Task.Delay(msDelay).Wait();

                // Check if exe exited or pipelines disconnected
                if (ProcessExited() || !PipelinesConnected())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Initialize file transmission and collection pipelines
        /// </summary>
        private List<StreamPipe> GetTransferPipes()
        {
            if (Program.Transfer is TransferOpt.None)
            {
                throw new ArgumentException(nameof(Program.Transfer));
            }
            FilePipe filePipe;

            // Check if receiving or sending file data
            if (Program.Transfer is TransferOpt.Collect)
            {
                filePipe = new FilePipe(_netReader, FilePath);
            }
            else
            {
                filePipe = new FilePipe(FilePath, _netWriter);
            }
            return new List<StreamPipe> { filePipe };
        }

        /// <summary>
        /// Initialize executable process pipelines
        /// </summary>
        private List<StreamPipe> GetProcessPipes()
        {
            return new List<StreamPipe>
            {
                new ProcessPipe(_netReader, _process.StandardInput),
                new ProcessPipe(_process.StandardOutput, _netWriter),
                new ProcessPipe(_process.StandardError, _netWriter)
            };
        }

        /// <summary>
        /// Initialize executable process pipelines
        /// </summary>
        private List<StreamPipe> GetTextPipes()
        {
            return new List<StreamPipe>
            {
                new TextPipe(Program.Payload, _netWriter)
            };
        }

        /// <summary>
        /// Initialize default socket stream pipelines
        /// </summary>
        private List<StreamPipe> GetDefaultPipes()
        {
            Stream stdin = Console.OpenStandardInput();
            Stream stdout = Console.OpenStandardOutput();

            return new List<StreamPipe>
            {
                new ProcessPipe(_netReader, new(stdout) { AutoFlush = true }),
                new ProcessPipe(new(stdin), _netWriter)
            };
        }

        /// <summary>
        /// Determine if command-shell has exited
        /// </summary>
        private bool ProcessExited()
        {
            return UsingExe && _process.HasExited;
        }

        /// <summary>
        /// Determine if all pipelines are connected/active
        /// </summary>
        private bool PipelinesConnected()
        {
            int nullCount = _pipes.Where(p => p is null).Count();

            if ((_pipes.Count == 0) || (nullCount == _pipes.Count))
            {
                return false;
            }

            // Check if non-null pipes are connected
            foreach (StreamPipe pipe in _pipes)
            {
                if ((pipe is not null) && !pipe.Connected)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
