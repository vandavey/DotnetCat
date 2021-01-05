﻿using System;
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

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Base class for all TCP socket nodes
    /// </summary>
    class SocketNode : IConnectable
    {
        private List<StreamPipe> _pipes;

        private Process _process;

        private StreamReader _netReader;
        private StreamWriter _netWriter;

        /// Initialize new object
        protected SocketNode()
        {
            Port = 4444;
            Verbose = false;
            OS = Program.OS;

            Cmd = new CommandHandler();
            Style = new StyleHandler();
            Error = new ErrorHandler();
            Client = new TcpClient();
        }

        /// Initialize new object
        protected SocketNode(IPAddress address) : this()
        {
            Addr = address;
        }

        protected enum PipeType : short { Default, File, Shell }

        public bool Verbose { get; set; }

        public int Port { get; set; }

        public string FilePath { get; set; }

        public string Exe { get; set; }

        public IPAddress Addr { get; set; }

        public TcpClient Client { get; set; }

        protected bool UsingExe => Exe != null;

        protected bool FSTransfer => Program.FileComm != Communicate.None;

        protected Platform OS { get; }

        protected CommandHandler Cmd { get; }

        protected ErrorHandler Error { get; }

        protected StyleHandler Style { get; }

        protected NetworkStream NetStream { get; set; }

        /// Initialize and run an executable process
        public bool Start(string exe = null)
        {
            Exe ??= Cmd.GetDefaultExe(OS);

            // Invalid executable path
            if (!Cmd.ExistsOnPath(exe).exists)
            {
                Error.Handle(Except.ExecPath, exe, true);
            }

            _process = new Process
            {
                StartInfo = GetStartInfo(exe)
            };
            return _process.Start();
        }

        /// Get start info to be used with executable process
        public ProcessStartInfo GetStartInfo(string shell)
        {
            _ = shell ?? throw new ArgNullException(nameof(shell));

            // Exe process startup information
            ProcessStartInfo info = new ProcessStartInfo(shell)
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
            if (OS is Platform.Win)
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
            if (UsingExe && FSTransfer)
            {
                string message = "--exec, --output/--send";
                Error.Handle(Except.ArgsCombo, message);
            }

            // Initialize and connect pipelines
            if (UsingExe)
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
            _pipes?.ForEach(pipe => pipe?.Connect());
        }

        /// Release any unmanaged resources
        public virtual void Dispose()
        {
            _pipes?.ForEach(pipe => pipe?.Dispose());

            _process?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            Client?.Dispose();
            NetStream?.Dispose();
        }

        /// Initialize socket stream pipelines
        protected void AddPipes(PipeType type)
        {
            _ = NetStream ?? throw new ArgNullException(nameof(NetStream));

            // Can't perform socket read/write operations
            if (!NetStream.CanRead || !NetStream.CanWrite)
            {
                string msg = "Can't perform stream read/write operations";
                throw new ArgumentException(msg, nameof(NetStream));
            }

            _netReader = new StreamReader(NetStream);
            _netWriter = new StreamWriter(NetStream);

            // Initialize socket pipeline(s)
            _pipes = type switch
            {
                PipeType.Shell => new List<StreamPipe>
                {
                    new ProcessPipe(_netReader, _process.StandardInput),
                    new ProcessPipe(_process.StandardOutput, _netWriter),
                    new ProcessPipe(_process.StandardError, _netWriter)
                },
                PipeType.File => new List<StreamPipe> { GetIOPipe() },
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

            // Connection closed status
            if (Verbose)
            {
                Style.Status($"Connection to {Addr}:{Port} closed");
            }
        }

        /// Initialize file transmission and collection pipelines
        private StreamPipe GetIOPipe()
        {
            if (Program.FileComm is Communicate.None)
            {
                throw new ArgumentException(nameof(Program.FileComm));
            }

            // Receiving file data
            if (Program.FileComm is Communicate.Collect)
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
                new ProcessPipe(new StreamReader(stdin), _netWriter),
                new ProcessPipe(_netReader, new StreamWriter(stdout))
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
