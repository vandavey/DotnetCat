using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotnetCat.Handlers;
using DotnetCat.Pipes;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Base class for SocketClient and SocketServer
    /// </summary>
    class SocketShell : ICloseable
    {
        private readonly List<StreamPipe> _pipes;

        private Process _shellProc;

        private StreamReader _netReader;
        private StreamWriter _netWriter;

        /// Initialize new SocketShell
        public SocketShell(IPAddress address = null)
        {
            _pipes = new List<StreamPipe>();

            this.Address = address;
            this.Port = 4444;
            this.IsVerbose = false;

            this.Cmd = new CommandHandler();
            this.Style = new StyleHandler();
            this.Error = new ErrorHandler();
            this.Client = new TcpClient();
        }

        public string FilePath { get; set; }

        public IPAddress Address { get; set; }

        public int Port { get; set; }

        public string Executable { get; set; }

        public bool IsVerbose { get; set; }

        public TcpClient Client { get; set; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected NetworkStream NetStream { get; set; }

        protected FileStream IOStream { get; set; }

        protected bool ShellHasExited
        {
            get => IsUsingShell() && _shellProc.HasExited;
        }

        /// Initialize new TCP stream socket
        public Socket CreateTcpSocket()
        {
            return new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp
            );
        }

        /// Initialize and start command shell process
        public bool StartProcess(string shell = null)
        {
            Executable ??= Cmd.GetDefaultShell();

            if (!Cmd.ExistsOnPath(shell).exists)
            {
                Error.Handle("shell", shell, true);
            }

            _shellProc = new Process
            {
                StartInfo = GetStartInfo(shell)
            };

            return _shellProc.Start();
        }

        /// Get start info to be used with shell process
        public ProcessStartInfo GetStartInfo(string shell)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(shell)
            {
                WorkingDirectory = Cmd.GetProfilePath(),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (Cmd.IsWindowsPlatform)
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
                throw new ArgumentNullException("NetStream");
            }

            if (IsUsingShell() && IsFileTransfer())
            {
                Error.Handle("combo", "--exec, --recv/--send");
            }

            if (IsUsingShell())
            {
                AddShellPipes();
            }
            else if (IsFileTransfer())
            {
                AddFilePipes();
            }
            else
            {
                AddDefaultPipes();
            }

            ConnectPipes(_pipes);
        }

        /// Release any unmanaged resources
        public virtual void Close()
        {
            _shellProc?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            _pipes.ForEach(pipe => pipe?.Close());

            Client?.Dispose();
            NetStream?.Dispose();
            IOStream?.Dispose();
        }

        /// Initialize StreamPipes
        protected void AddDefaultPipes()
        {
            Stream consoleStdin = Console.OpenStandardInput();
            Stream consoleStdout = Console.OpenStandardOutput();

            StreamReader stdin = new StreamReader(consoleStdin);
            StreamWriter stdout = new StreamWriter(consoleStdout);

            InitStreamHandlers(NetStream);

            _pipes.Add(new ShellPipe(stdin, _netWriter));
            _pipes.Add(new ShellPipe(_netReader, stdout));
        }

        /// Initialize ShellPipes
        protected void AddShellPipes()
        {
            StreamWriter shellStdin = _shellProc.StandardInput;
            StreamReader shellStdout = _shellProc.StandardOutput;
            StreamReader shellErr = _shellProc.StandardError;

            InitStreamHandlers(NetStream);

            _pipes.Add(new ShellPipe(_netReader, shellStdin));
            _pipes.Add(new ShellPipe(shellStdout, _netWriter));
            _pipes.Add(new ShellPipe(shellErr, _netWriter));
        }

        /// Initialize socket stream reader and writer
        protected void InitStreamHandlers(NetworkStream stream)
        {
            _netReader = new StreamReader(stream);
            _netWriter = new StreamWriter(stream);
        }

        /// Initialize FilePipes
        protected void AddFilePipes()
        {
            if (Program.TransferType == "recv")
            {
                _netReader = new StreamReader(NetStream);
                _pipes.Add(new FilePipe(_netReader, FilePath));
                return;
            }

            _netWriter = new StreamWriter(NetStream);
            _pipes.Add(new FilePipe(FilePath, _netWriter));
        }

        /// Call the Connect method for all StreamPipes
        protected void ConnectPipes(List<StreamPipe> pipes)
        {
            pipes.ForEach(pipe => pipe?.Connect());
        }

        /// Determine if node is using shell executable
        protected bool IsUsingShell() => Executable != null;

        /// Determine if file transfer is specified
        protected bool IsFileTransfer()
        {
            return Program.TransferType != null;
        }

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
            int nullCount = _pipes.Where(x => x == null).Count();

            if ((_pipes.Count == 0) || (nullCount == _pipes.Count()))
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
