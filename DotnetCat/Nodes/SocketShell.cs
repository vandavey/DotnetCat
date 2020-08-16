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
using DotnetCat.Utils;

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
        protected SocketShell(IPAddress address = null)
        {
            _pipes = new List<StreamPipe>();

            this.Address = address;
            this.Port = 4444;
            this.IsVerbose = false;

            this.Cmd = new CommandHandler();
            this.Style = new StyleHandler();
            this.Error = new ErrorHandler();
            this.Client = new TcpClient();

            SysPlatform = Program.GetPlatform();
        }

        public enum PipeType { Default, Shell, Transfer }

        public string ShellPath { get; set; }

        public IPAddress Address { get; set; }

        public int Port { get; set; }

        public string Executable { get; set; }

        public bool IsVerbose { get; set; }

        public Platform SysPlatform { get; set; }

        public TcpClient Client { get; set; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected NetworkStream NetStream { get; set; }

        protected bool IsUsingShell { get => Executable != null; }

        /// Determine if file transfer is specified
        protected bool IsFileTransfer
        {
            get => Program.SocketAction != NodeAction.None;
        }

        private bool ShellHasExited
        {
            get => IsUsingShell && _shellProc.HasExited;
        }

        /// Initialize and start command shell process
        public bool StartProcess(string shell = null)
        {
            Executable ??= Cmd.GetDefaultShell(SysPlatform);

            if (!Cmd.ExistsOnPath(shell).exists)
            {
                Error.Handle(ErrorType.ShellPath, shell, true);
            }

            _shellProc = new Process
            {
                StartInfo = GetStartInfo(shell),
            };

            return _shellProc.Start();
        }

        /// Get start info to be used with shell process
        public ProcessStartInfo GetStartInfo(string shell)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(shell)
            {
                CreateNoWindow = true,
                WorkingDirectory = Cmd.GetProfilePath(SysPlatform),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            if (SysPlatform == Platform.Windows)
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

            if (IsUsingShell && IsFileTransfer)
            {
                string message = "--exec, --recv/--send";
                Error.Handle(ErrorType.ArgCombination, message);
            }

            if (IsUsingShell)
            {
                AddPipes(PipeType.Shell);
            }
            else if (IsFileTransfer)
            {
                AddPipes(PipeType.Transfer);
            }
            else
            {
                AddPipes(PipeType.Default);
            }

            _pipes.ForEach(pipe => pipe?.Connect());
        }

        /// Release any unmanaged resources
        public virtual void Close()
        {
            _pipes.ForEach(pipe => pipe?.Close());

            _shellProc?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            Client?.Dispose();
            NetStream?.Dispose();
        }

        /// Initialize specified pipes
        protected void AddPipes(PipeType option)
        {
            _netReader = new StreamReader(NetStream);
            _netWriter = new StreamWriter(NetStream);

            if (option == PipeType.Shell)
            {
                AddShellPipes();
            }
            else if (option == PipeType.Transfer)
            {
                AddFilePipes();
            }
            else
            {
                AddDefaultPipes();
            }
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

        /// Initialize ShellPipes
        private void AddShellPipes()
        {
            StreamWriter shellStdin = _shellProc.StandardInput;
            StreamReader shellStdout = _shellProc.StandardOutput;
            StreamReader shellErr = _shellProc.StandardError;

            _pipes.Add(new ShellPipe(_netReader, shellStdin));
            _pipes.Add(new ShellPipe(shellStdout, _netWriter));
            _pipes.Add(new ShellPipe(shellErr, _netWriter));
        }

        /// Initialize FilePipes
        private void AddFilePipes()
        {
            if (Program.SocketAction == NodeAction.RecvFile)
            {
                _netReader = new StreamReader(NetStream);
                _pipes.Add(new FilePipe(_netReader, ShellPath));
                return;
            }

            _netWriter = new StreamWriter(NetStream);
            _pipes.Add(new FilePipe(ShellPath, _netWriter));
        }

        /// Initialize StreamPipes
        private void AddDefaultPipes()
        {
            Stream consoleStdin = Console.OpenStandardInput();
            Stream consoleStdout = Console.OpenStandardOutput();

            StreamReader stdin = new StreamReader(consoleStdin);
            StreamWriter stdout = new StreamWriter(consoleStdout);

            _pipes.Add(new ShellPipe(stdin, _netWriter));
            _pipes.Add(new ShellPipe(_netReader, stdout));
        }

        /// Determine if all pipes are connected
        private bool AllPipesConnected()
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
