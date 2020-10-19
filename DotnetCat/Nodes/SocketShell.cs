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
    /// Base class for SocketClient and SocketServer
    /// </summary>
    class SocketShell : IConnectable
    {
        private readonly List<StreamPipe> _pipes;

        private Process _shellProc;

        private StreamReader _netReader;
        private StreamWriter _netWriter;

        /// Initialize new SocketShell
        protected SocketShell(IPAddress address = null)
        {
            _pipes = new List<StreamPipe>();
            this.Platform = Program.Platform;

            this.Address = address;
            this.Port = 4444;
            this.Verbose = false;

            this.Cmd = new CommandHandler();
            this.Style = new StyleHandler();
            this.Error = new ErrorHandler();
            this.Client = new TcpClient();
        }

        protected enum PipeType { Default, FileSys, Shell }

        public string FilePath { get; set; }

        public IPAddress Address { get; set; }

        public int Port { get; set; }

        public string Executable { get; set; }

        public bool Verbose { get; set; }

        public TcpClient Client { get; set; }

        protected PlatformType Platform { get; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected NetworkStream NetStream { get; set; }

        protected bool UsingShell { get => Executable != null; }

        protected bool FileSysTransfer
        {
            get => Program.IOAction != IOActionType.None;
        }

        /// Initialize and start command shell process
        public bool StartProcess(string shell = null)
        {
            Executable ??= Cmd.GetDefaultShell(Platform);

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
                WorkingDirectory = Cmd.GetProfilePath(Platform),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            if (Platform == PlatformType.Windows)
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

            if (UsingShell && FileSysTransfer)
            {
                string message = "--exec, --output/--send";
                Error.Handle(ErrorType.ArgCombination, message);
            }

            if (UsingShell)
            {
                AddPipes(PipeType.Shell);
            }
            else if (FileSysTransfer)
            {
                AddPipes(PipeType.FileSys);
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

        /// Initialize specified pipes
        protected void AddPipes(PipeType option)
        {
            _netReader = new StreamReader(NetStream);
            _netWriter = new StreamWriter(NetStream);

            switch (option)
            {
                case PipeType.Shell:
                    AddShellPipes();
                    break;
                case PipeType.FileSys:
                    AddIOPipes();
                    break;
                case PipeType.Default:
                    AddDefaultPipes();
                    break;
            }
        }

        /// Wait for pipes to be disconnected
        protected void WaitForExit()
        {
            while (Client.Connected)
            {
                Task.Delay(100).Wait();

                if (ShellExited() || !AllPipesConnected())
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
            StreamReader shellStderr = _shellProc.StandardError;

            _pipes.Add(new ShellPipe(_netReader, shellStdin));
            _pipes.Add(new ShellPipe(shellStdout, _netWriter));
            _pipes.Add(new ShellPipe(shellStderr, _netWriter));
        }

        /// Initialize IOPipes derived pipes
        private void AddIOPipes()
        {
            if (Program.IOAction == IOActionType.None)
            {
                throw new ArgumentException(nameof(Program.IOAction));
            }

            if (Program.IOAction == IOActionType.WriteFile)
            {
                _netReader = new StreamReader(NetStream);
                _pipes.Add(new FilePipe(_netReader, FilePath));
                return;
            }

            if (Program.Recursive)
            {
            }
            _netWriter = new StreamWriter(NetStream);
            _pipes.Add(new FilePipe(FilePath, _netWriter));
        }

        /// Initialize StreamPipes
        private void AddDefaultPipes()
        {
            Stream stdinStream = Console.OpenStandardInput();
            Stream stdoutStream = Console.OpenStandardOutput();

            StreamReader stdin = new StreamReader(stdinStream);
            StreamWriter stdout = new StreamWriter(stdoutStream);

            _pipes.Add(new ShellPipe(stdin, _netWriter));
            _pipes.Add(new ShellPipe(_netReader, stdout));
        }

        /// Determine if command-shell has exited
        private bool ShellExited()
        {
            return UsingShell && _shellProc.HasExited;
        }

        /// Determine if all pipes are connected/active
        private bool AllPipesConnected()
        {
            int nullCount = _pipes.Where(x => x == null).Count();

            if ((_pipes.Count == 0) || (nullCount == _pipes.Count))
            {
                return false;
            }

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
