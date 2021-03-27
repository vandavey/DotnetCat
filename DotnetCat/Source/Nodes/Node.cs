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

        /// Initialize object
        protected Node()
        {
            Port = 4444;
            Verbose = false;
            Client = new TcpClient();
        }

        /// Initialize object
        protected Node(IPAddress address) : this()
        {
            Addr = address;
        }

        /// Cleanup resources
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

        /// Initialize and run an executable process
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

        /// Get ProcessStartInfo to use for executable startup
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

        /// Activate communication between pipe streams
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

        /// Dispose of unmanaged socket resources and handle error
        public virtual void PipeError(Except type, IPEndPoint ep,
                                                   Exception ex = null,
                                                   Level level = Level.Error) {
            PipeError(type, ep.ToString(), ex, level);
        }

        /// Dispose of unmanaged resources and handle error
        public virtual void PipeError(Except type, string arg,
                                                   Exception ex = null,
                                                   Level level = Level.Error) {
            Dispose();
            ErrorHandler.Handle(type, arg, ex, level);
        }

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

        /// Initialize socket stream pipelines
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

        /// Wait for pipeline(s) to be disconnected
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

        /// Initialize file transmission and collection pipelines
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

        /// Initialize executable process pipelines
        private List<StreamPipe> GetProcessPipes()
        {
            return new List<StreamPipe>
            {
                new ProcessPipe(_netReader, _process.StandardInput),
                new ProcessPipe(_process.StandardOutput, _netWriter),
                new ProcessPipe(_process.StandardError, _netWriter)
            };
        }

        /// Initialize executable process pipelines
        private List<StreamPipe> GetTextPipes()
        {
            return new List<StreamPipe>
            {
                new TextPipe(Program.Payload, _netWriter)
            };
        }

        /// Initialize default socket stream pipelines
        private List<StreamPipe> GetDefaultPipes()
        {
            Stream stdin = Console.OpenStandardInput();
            Stream stdout = Console.OpenStandardOutput();

            return new List<StreamPipe>
            {
                new ProcessPipe(_netReader, new StreamWriter(stdout)
                {
                    AutoFlush = true
                }),
                new ProcessPipe(new StreamReader(stdin), _netWriter)
            };
        }

        /// Determine if command-shell has exited
        private bool ProcessExited()
        {
            return UsingExe && _process.HasExited;
        }

        /// Determine if all pipelines are connected/active
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
