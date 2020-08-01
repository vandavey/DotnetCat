using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DotnetCat
{
    /// <summary>
    /// Base class for SocketClient and SocketServer
    /// </summary>
    class SocketShell : IPipeHandler
    {
        private Process _shellProc;

        private StreamPipe _inputPipe;
        private StreamPipe _outputPipe;
        private StreamPipe _errorPipe;

        private StreamPipe[] _pipes;

        /// Initialize new SocketShell
        public SocketShell(IPAddress addr)
        {
            this.Address = addr;
            this.Port = 4444;
            this.Verbose = false;

            this.Cmd = new CommandHandler();
            this.Style = new StyleHandler();
            this.Error = new ErrorHandler();

            this.Client = new TcpClient();
        }

        public IPAddress Address { get; set; }

        public int Port { get; set; }

        public string Shell { get; set; }

        public bool Verbose { get; set; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected TcpClient Client { get; set; }

        protected NetworkStream NetStream { get; set; }

        protected bool ShellHasExited
        {
            get => IsUsingShell() && _shellProc.HasExited;
        }

        /// Initialize and return new TCP stream socket
        public Socket NewTcpSocket()
        {
            return new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp
            );
        }

        /// Initialize and start command shell process
        public bool StartProcess(string shell = null)
        {
            Shell ??= Cmd.DefaultShell();

            if (!Cmd.ExistsOnPath(shell).exists)
            {
                Error.Handle("shell", shell, true);
            }

            _shellProc = new Process
            {
                StartInfo = new ProcessStartInfo(shell)
                {
                    WorkingDirectory = Cmd.GetProfilePath(),
                    LoadUserProfile = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };

            return _shellProc.Start();
        }

        /// Activate communication between pipe streams
        public void ConnectPipes()
        {
            if (NetStream == null)
            {
                throw new ArgumentNullException("NetStream");
            }

            InitializePipes();

            _inputPipe?.Connect();
            _outputPipe?.Connect();
            _errorPipe?.Connect();
        }

        /// Wait for pipes to be disconnected
        public void WaitForExit()
        {
            while (Client.Connected)
            {
                Task.Delay(100).Wait();

                if (ShellHasExited || !AllPipesConnected())
                {
                    break;
                }
            }
        }

        /// Determine if all pipes are connected
        public bool AllPipesConnected()
        {
            int? nullCount = _pipes?.Where(x => x == null).Count();

            if ((nullCount == null) || (nullCount == 3))
            {
                return false;
            }

            foreach (StreamPipe pipe in _pipes)
            {
                if ((pipe != null) && !pipe.IsConnected)
                {
                    return false;
                }
            }

            return true;
        }

        /// Release any unmanaged resources
        public virtual void Close()
        {
            _inputPipe?.Close();
            _outputPipe?.Close();
            _errorPipe?.Close();

            Client?.Dispose();
            NetStream?.Dispose();
            _shellProc?.Dispose();
        }

        /// Initialize the stream pipes
        protected void InitializePipes(NetworkStream stream = null)
        {
            stream ??= NetStream;
            Stream stdInput, stdOutput, stdError;

            if (IsUsingShell())
            {
                stdInput = _shellProc.StandardInput.BaseStream;
                stdOutput = _shellProc.StandardOutput.BaseStream;
                stdError = _shellProc.StandardError.BaseStream;

                _inputPipe = new StreamPipe(Client, stream, stdInput);
                _outputPipe = new StreamPipe(Client, stdOutput, stream);
                _errorPipe = new StreamPipe(Client, stdError, stream);
            }
            else
            {
                stdInput = Console.OpenStandardInput();
                stdOutput = Console.OpenStandardOutput();

                _inputPipe = new StreamPipe(Client, stdInput, stream);
                _outputPipe = new StreamPipe(Client, stream, stdOutput);
            }

            _pipes = new StreamPipe[]
            {
                _inputPipe, _outputPipe, _errorPipe
            };
        }

        /// Determine if node is using shell executable
        protected bool IsUsingShell() => Shell != null;
    }
}
