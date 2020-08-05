using DotnetCat.Handlers;
using DotnetCat.Pipes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Base class for SocketClient and SocketServer
    /// </summary>
    class SocketShell : ICloseable
    {
        private Process _shellProc;

        private StreamPipe _inputPipe;
        private StreamPipe _outputPipe;
        private StreamPipe _errorPipe;

        private StreamPipe[] _pipes;

        /// Initialize new SocketShell
        public SocketShell(string tansferType, string path = null)
        {
            this.TransferType = tansferType;
            this.FilePath = path;
            this.Port = 4444;
            this.Verbose = false;

            this.Cmd = new CommandHandler();
            this.Style = new StyleHandler();
            this.Error = new ErrorHandler();

            this.Client = new TcpClient();
        }

        public string TransferType { get; set; }

        public string FilePath { get; set; }

        public IPAddress Address { get; set; }

        public int Port { get; set; }

        public string Shell { get; set; }

        public bool Verbose { get; set; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected TcpClient Client { get; set; }

        protected NetworkStream NetStream { get; set; }
        
        protected FileStream IOStream { get; set; }

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
            Shell ??= Cmd.GetDefaultShell();

            if (!Cmd.ExistsOnPath(shell).exists)
            {
                Error.Handle("shell", shell, true);
            }

            if (Cmd.IsWindowsPlatform)
            {
                _shellProc = new Process
                {
                    StartInfo = GetStartInfo(shell, true)
                };
            }
            else
            {
                _shellProc = new Process
                {
                    StartInfo = GetStartInfo(shell, false)
                };
            }

            return _shellProc.Start();
        }

        /// Get start info to be used with shell process
        public ProcessStartInfo GetStartInfo(string shell, bool loadProf)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(shell)
            {
                WorkingDirectory = Cmd.GetProfilePath(),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (loadProf)
            {
                startInfo.LoadUserProfile = true;
            }

            return startInfo;
        }

        /// Activate communication between pipe streams
        public void ConnectPipes()
        {
            if (NetStream == null)
            {
                throw new ArgumentNullException("NetStream");
            }

            if (IsUsingShell() && IsFileTransfer())
            {
                Error.Handle("combo", "--exec, --recv/--send");
            }

            if (IsUsingShell())
            {
                _pipes = InitializePipes(_shellProc);
            }
            else if (IsFileTransfer())
            {
                if (TransferType == "recv")
                {
                    _pipes = InitializePipes(OpenFile(FileAccess.Write));
                }

                _pipes = InitializePipes(OpenFile(FileAccess.Read));
            }
            else
            {
                _pipes = InitializePipes();
            }

            _inputPipe?.Connect();
            _outputPipe?.Connect();
            _errorPipe?.Connect();
        }
        
        /// Release any unmanaged resources
        public virtual void Close()
        {
            _inputPipe?.Close();
            _outputPipe?.Close();
            _errorPipe?.Close();

            Client?.Dispose();
            NetStream?.Dispose();
            IOStream?.Dispose();
            _shellProc?.Dispose();
        }

        /// Initialize the stream pipes
        protected StreamPipe[] InitializePipes()
        {
            Stream input = Console.OpenStandardInput();
            Stream output = Console.OpenStandardOutput();

            return new StreamPipe[]
            {
                _inputPipe = new StreamPipe(Client, input, NetStream),
                _outputPipe = new StreamPipe(Client, NetStream, output),
                _errorPipe = null
            };
        }

        /// Initialize the stream pipes
        protected StreamPipe[] InitializePipes(Process process)
        {
            Stream input = process.StandardInput.BaseStream;
            Stream output = process.StandardOutput.BaseStream;
            Stream error = process.StandardError.BaseStream;

            return new StreamPipe[]
            {
                _inputPipe = new StreamPipe(Client, NetStream, input),
                _outputPipe = new StreamPipe(Client, output, NetStream),
                _errorPipe = new StreamPipe(Client, error, NetStream)
            };
        }

        /// Initialize the stream pipes
        protected StreamPipe[] InitializePipes(FileStream stream)
        {
            if (TransferType == "recv")
            {
                _outputPipe = new StreamPipe(Client, NetStream, stream)
                {
                    IsFileTransfer = true
                };
            }
            else
            {
                _inputPipe = new StreamPipe(Client, stream, NetStream)
                {
                    IsFileTransfer = true
                };
            }

            return new StreamPipe[] { _inputPipe, _outputPipe, _errorPipe };
        }

        /// Open specified FileStream for reading/writing
        public FileStream OpenFile(FileAccess access)
        {
            FileMode fileMode;
            FileShare fileShare;

            IOStream?.Dispose();

            if (access == FileAccess.Write)
            {
                fileMode = FileMode.OpenOrCreate;
                fileShare = FileShare.Write;
            }
            else
            {
                fileMode = FileMode.Open;
                fileShare = FileShare.Read;
            }

            return IOStream = new FileStream(
                FilePath, fileMode, access, fileShare,
                bufferSize: 4096, useAsync: true
            );
        }

        /// Determine if node is using shell executable
        protected bool IsUsingShell() => Shell != null;
        
        /// Determine if file transfer is specified
        protected bool IsFileTransfer() => TransferType != null;

        /// Wait for pipes to be disconnected
        protected void WaitForExit()
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
        protected bool AllPipesConnected()
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
    }
}
