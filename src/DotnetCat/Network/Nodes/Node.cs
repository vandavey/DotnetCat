using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.FileSystem;
using DotnetCat.Pipelines;
using DotnetCat.Shell.Commands;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes
{
    /// <summary>
    ///  Base class for all socket nodes in Nodes namespace
    /// </summary>
    internal class Node : ISockErrorHandled
    {
        private bool _validArgCombos;     // Valid cmd-line arg combos

        private string _destName;         // Target host name

        private Process _process;         // Executable process

        private StreamReader _netReader;  // TCP stream reader

        private StreamWriter _netWriter;  // TCP stream writer

        private List<Pipeline> _pipes;    // Pipeline list

        /// <summary>
        ///  Initialize object
        /// </summary>
        protected Node()
        {
            _validArgCombos = false;
            _destName = default;
            _process = default;
            _netReader = default;
            _netWriter = default;
            _pipes = default;

            Port = 44444;
            Verbose = false;
            Client = new TcpClient();
        }

        /// <summary>
        ///  Initialize object
        /// </summary>
        protected Node(IPAddress address) : this()
        {
            Addr = address;
            DestName = Addr.ToString();
        }

        /// <summary>
        ///  Cleanup resources
        /// </summary>
        ~Node() => Dispose();

        /// Enable verbose console output
        public bool Verbose { get; set; }

        /// Network port number
        public int Port { get; set; }

        /// Executable file path
        public string Exe { get; set; }

        /// Transfer file path
        public string FilePath { get; set; }

        /// Destination host name
        public string DestName
        {
            get => _destName ?? ((Addr is null) ? "" : Addr.ToString());
            set
            {
                _destName = value ?? ((Addr is null) ? "" : Addr.ToString());
            }
        }

        /// IPv4 address
        public IPAddress Addr { get; set; }

        /// TCP network client
        public TcpClient Client { get; set; }

        /// Performing file transfer
        protected static bool Transfer
        {
            get => Program.Transfer is not TransferOpt.None;
        }

        /// Using executable pipeline
        protected bool UsingExe => !Exe.IsNullOrEmpty();

        /// TCP network stream
        protected NetworkStream NetStream { get; set; }

        /// <summary>
        ///  Initialize and run an executable process
        /// </summary>
        public bool StartProcess(string exe)
        {
            (string path, bool exists) = FileSys.ExistsOnPath(exe);

            if (!exists)
            {
                Dispose();
                Error.Handle(Except.ExePath, exe, true);
            }

            _process = new Process
            {
                StartInfo = Command.GetExeStartInfo(Exe = path)
            };
            return _process.Start();
        }

        /// <summary>
        ///  Activate communication between pipe streams
        /// </summary>
        public virtual void Connect()
        {
            _ = NetStream ?? throw new ArgumentNullException(nameof(NetStream));

            if (!_validArgCombos)
            {
                ValidateArgCombinations();
            }

            AddPipes(Program.PipeVariant);
            _pipes?.ForEach(pipe => pipe?.Connect());
        }

        /// <summary>
        ///  Dispose of unmanaged socket resources and handle error
        /// </summary>
        public virtual void PipeError(Except type,
                                      HostEndPoint target,
                                      Exception ex = default,
                                      Level level = default) {
            Dispose();
            Error.Handle(type, target.ToString(), ex, level);
        }

        /// <summary>
        ///  Dispose of unmanaged resources and handle error
        /// </summary>
        public virtual void PipeError(Except type,
                                      string arg,
                                      Exception ex = default,
                                      Level level = default) {
            Dispose();
            Error.Handle(type, arg, ex, level);
        }

        /// <summary>
        ///  Release any unmanaged resources
        /// </summary>
        public virtual void Dispose()
        {
            _pipes?.ForEach(pipe => pipe?.Dispose());

            _process?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            Client?.Close();
            NetStream?.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Validate command-line argument combinations
        /// </summary>
        protected void ValidateArgCombinations()
        {
            // Combinations already validated
            if (_validArgCombos)
            {
                return;
            }

            // Combination: --exec, --output/--send
            if (UsingExe && Transfer)
            {
                Console.WriteLine(Parser.Usage);
                PipeError(Except.ArgsCombo, "--exec, --output/--send");
            }

            bool isTextPipe = !Program.Payload.IsNullOrEmpty();

            // Combination: --exec, --text
            if (UsingExe && isTextPipe)
            {
                Console.WriteLine(Parser.Usage);
                PipeError(Except.ArgsCombo, "--exec, --text");
            }

            // Combination: --text, --output/--send
            if (isTextPipe && Transfer)
            {
                Console.WriteLine(Parser.Usage);
                PipeError(Except.ArgsCombo, "--text, --output/--send");
            }

            if (Parser.IndexOfFlag("--zero-io", Program.OrigArgs) != -1)
            {
                // Invalid combo: --listen, --zero-io
                if (Program.SockNode is ServerNode)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--listen, --zero-io");
                }

                // Combination: --zero-io, --text
                if (isTextPipe)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--zero-io, --text");
                }

                // Combination: --zero-io, --output/--send
                if (Transfer)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--zero-io, --output/--send");
                }

                // Combination: --exec, --zero-io
                if (UsingExe)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--exec, --zero-io");
                }
            }

            _validArgCombos = true;
        }

        /// <summary>
        ///  Initialize socket stream pipelines
        /// </summary>
        protected void AddPipes(PipeType pipeType)
        {
            // Invalid network stream
            if (NetStream is null || !NetStream.CanRead || !NetStream.CanWrite)
            {
                throw new ArgumentException(nameof(NetStream));
            }

            _netWriter = new StreamWriter(NetStream)
            {
                AutoFlush = true
            };
            _netReader = new StreamReader(NetStream);

            // Initialize socket pipeline(s)
            _pipes = pipeType switch
            {
                PipeType.File        => GetPipelines(PipeType.File),
                PipeType.Process     => GetPipelines(PipeType.Process),
                PipeType.Status      => GetPipelines(PipeType.Status),
                PipeType.Text        => GetPipelines(PipeType.Text),
                PipeType.Stream or _ => GetPipelines(PipeType.Stream)
            };
        }

        /// <summary>
        ///  Wait for pipeline(s) to be disconnected
        /// </summary>
        protected void WaitForExit(int msDelay = 100)
        {
            while (Client.Connected)
            {
                Task.Delay(msDelay).Wait();

                // Check if process exited or pipelines disconnected
                if (ProcessExited() || !PipelinesConnected())
                {
                    break;
                }
            }
        }

        /// <summary>
        ///  Initialize underlying pipelines
        /// </summary>
        private List<Pipeline> GetPipelines(PipeType type)
        {
            List<Pipeline> pipes = new();

            switch (type)
            {
                case PipeType.Stream:   // Stream pipelines
                {
                    Stream stdInput = Console.OpenStandardInput();
                    Stream stdOutput = Console.OpenStandardOutput();

                    // Add stream pipelines
                    pipes.AddRange(new StreamPipe[]
                    {
                        new StreamPipe(_netReader, new StreamWriter(stdOutput)
                        {
                            AutoFlush = true
                        }),
                        new StreamPipe(new StreamReader(stdInput), _netWriter)
                    });
                    break;
                }
                case PipeType.File:    // File-transfer pipeline
                {
                    if (Program.Transfer is TransferOpt.None)
                    {
                        throw new ArgumentException(nameof(Program.Transfer));
                    }

                    // Add file-transfer stream pipe
                    if (Program.Transfer is TransferOpt.Collect)
                    {
                        pipes.Add(new FilePipe(_netReader, FilePath));
                    }
                    else
                    {
                        pipes.Add(new FilePipe(FilePath, _netWriter));
                    }
                    break;
                }
                case PipeType.Process:  // Process pipelines
                {
                    pipes.AddRange(new ProcessPipe[]
                    {
                        new ProcessPipe(_netReader, _process.StandardInput),
                        new ProcessPipe(_process.StandardOutput, _netWriter),
                        new ProcessPipe(_process.StandardError, _netWriter)
                    });
                    break;
                }
                case PipeType.Status:   // Zero-IO pipeline
                {
                    pipes.Add(new StatusPipe(_netWriter));
                    break;
                }
                case PipeType.Text:     // Text pipeline
                {
                    pipes.Add(new TextPipe(Program.Payload, _netWriter));
                    break;
                }
                default:
                {
                    break;
                }
            }

            return pipes;
        }

        /// <summary>
        ///  Determine if command-shell has exited
        /// </summary>
        private bool ProcessExited() => UsingExe && _process.HasExited;

        /// <summary>
        ///  Determine if all pipelines are connected/active
        /// </summary>
        private bool PipelinesConnected()
        {
            int nullCount = _pipes.Where(p => p is null).Count();

            if ((_pipes.Count == 0) || (nullCount == _pipes.Count))
            {
                return false;
            }

            // Check if non-null pipes are connected
            foreach (Pipeline pipe in _pipes)
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
