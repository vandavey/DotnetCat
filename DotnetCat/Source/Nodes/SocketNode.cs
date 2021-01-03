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

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Base class for all TCP socket nodes
    /// </summary>
    class SocketNode : IConnectable
    {
        private List<StreamPipe> _pipes;

        private Process _shellProc;

        private StreamReader _netReader;
        private StreamWriter _netWriter;

        /// Initialize new object
        protected SocketNode(IPAddress address = null)
        {
            _pipes = null;
            this.OS = Program.OS;

            this.Addr = address;
            this.Port = 4444;
            this.Verbose = false;

            this.Cmd = new CommandHandler();
            this.Style = new StyleHandler();
            this.Error = new ErrorHandler();
            this.Client = new TcpClient();
        }

        protected enum PipeType : short { Default, File, Shell }

        public bool Verbose { get; set; }

        public int Port { get; set; }

        public string FilePath { get; set; }

        public string Exe { get; set; }

        public IPAddress Addr { get; set; }

        public TcpClient Client { get; set; }

        protected bool UsingShell => Exe != null;

        protected bool FSTransfer => Program.NetOpt != Communicate.None;

        protected Platform OS { get; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected NetworkStream NetStream { get; set; }

        /// Initialize and run an executable process
        public bool Start(string shell = null)
        {
            Exe ??= Cmd.GetDefaultExe(OS);

            // Invalid executable path
            if (!Cmd.ExistsOnPath(shell).exists)
            {
                Error.Handle(Except.ShellPath, shell, true);
            }

            _shellProc = new Process { StartInfo = GetStartInfo(shell) };
            return _shellProc.Start();
        }

        /// Get start info to be used with shell process
        public ProcessStartInfo GetStartInfo(string shell)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(shell)
            {
                CreateNoWindow = true,
                WorkingDirectory = Cmd.GetProfilePath(OS),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            if (OS == Platform.Windows)
            {
                startInfo.LoadUserProfile = true;
            }
            return startInfo;
        }

        /// Activate communication between pipe streams
        public virtual void Connect()
        {
            if (NetStream == null)
            {
                throw new ArgumentNullException(nameof(NetStream));
            }

            // Invalid argument combination
            if (UsingShell && FSTransfer)
            {
                string message = "--exec, --output/--send";
                Error.Handle(Except.ArgCombination, message);
            }

            // Initialize and connect pipelines
            if (UsingShell)
            {
                AddPipes(PipeType.Shell);
            }
            else if (FSTransfer)
            {
                AddPipes(PipeType.File);
            }
            else
            {
                AddPipes(PipeType.Default);
            }
            _pipes.ForEach(pipe => pipe?.Connect());
        }

        /// Release any unmanaged resources
        public virtual void Dispose()
        {
            _pipes.ForEach(pipe => pipe?.Dispose());

            _shellProc?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            Client?.Dispose();
            NetStream?.Dispose();
        }

        /// Initialize socket stream pipelines
        protected void AddPipes(PipeType option)
        {
            if (NetStream == null)
            {
                throw new ArgumentNullException(nameof(NetStream));
            }

            // Can't perform stream read/write operations
            if (!NetStream.CanRead || !NetStream.CanWrite)
            {
                string msg = "Can't perform stream read/write operations";
                throw new ArgumentException(msg, nameof(NetStream));
            }

            _netReader = new StreamReader(NetStream);
            _netWriter = new StreamWriter(NetStream);

            // Initialize the socket pipeline(s)
            _pipes = option switch
            {
                PipeType.File => new List<StreamPipe>
                {
                    GetIOPipe()
                },
                PipeType.Shell => new List<StreamPipe>
                {
                    new ShellPipe(_netReader, _shellProc.StandardInput),
                    new ShellPipe(_shellProc.StandardOutput, _netWriter),
                    new ShellPipe(_shellProc.StandardError, _netWriter)
                },
                _ => GetDefaultPipes()
            };
        }

        /// Wait for pipeline(s) to be disconnected
        protected void WaitForExit()
        {
            while (Client.Connected)
            {
                Task.Delay(100).Wait();

                // Check if exe exited or pipelines disconnected
                if (ProcessExited() || !PipelinesConnected())
                {
                    break;
                }
            }
        }

        /// Initialize file transmission and collection pipelines
        private StreamPipe GetIOPipe()
        {
            if (Program.NetOpt == Communicate.None)
            {
                throw new ArgumentException(nameof(Program.NetOpt));
            }

            // Receiving file data
            if (Program.NetOpt == Communicate.Collect)
            {
                return new FilePipe(_netReader, FilePath);
            }

            // Sending file data
            if (Program.Recursive)
            {
                return new ArchivePipe(FilePath);
            }
            return new FilePipe(FilePath, _netWriter);
        }

        /// Initialize default socket stream pipelines
        private List<StreamPipe> GetDefaultPipes()
        {
            Stream stdin = Console.OpenStandardInput();
            Stream stdout = Console.OpenStandardOutput();

            return new List<StreamPipe>
            {
                new ShellPipe(new StreamReader(stdin), _netWriter),
                new ShellPipe(_netReader, new StreamWriter(stdout))
            };
        }

        /// Determine if command-shell has exited
        private bool ProcessExited()
        {
            return UsingShell && _shellProc.HasExited;
        }

        /// Determine if all pipelines are connected/active
        private bool PipelinesConnected()
        {
            int nullCount = _pipes.Where(p => p == null).Count();

            if ((_pipes.Count == 0) || (nullCount == _pipes.Count))
            {
                return false;
            }

            // Check if non-null pipes are connected
            foreach (StreamPipe pipe in _pipes)
            {
                if ((pipe != null) && !pipe.Connected)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
