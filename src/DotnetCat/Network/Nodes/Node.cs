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
using DotnetCat.IO.Pipelines;
using DotnetCat.Shell.Commands;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes
{
    /// <summary>
    ///  Base class for all socket nodes in the DotnetCat.Network.Nodes namespace.
    /// </summary>
    internal abstract class Node : ISockErrorHandled
    {
        private bool _validArgsCombos;     // Valid cmd-line arg combos

        private string? _hostName;         // Target hostname

        private Process? _process;         // Executable process

        private StreamReader? _netReader;  // TCP stream reader

        private StreamWriter? _netWriter;  // TCP stream writer

        private List<Pipeline>? _pipes;    // Pipeline list

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        protected Node()
        {
            _hostName = default;
            _netReader = default;
            _netWriter = default;
            _pipes = default;
            _process = default;
            _validArgsCombos = false;

            Args = new CmdLineArgs();
            Client = new TcpClient();

            Port = 44444;
            Verbose = false;
        }

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        protected Node(IPAddress addr, int port = 44444) : this()
        {
            Address = addr;
            HostName = addr.ToString();
            Port = port;
        }

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        protected Node(CmdLineArgs args) : this() => Args = args;

        /// <summary>
        ///  Release the unmanaged object resources.
        /// </summary>
        ~Node() => Dispose();

        /// Enable verbose console output
        public bool Verbose
        {
            get => Args.Verbose;
            set => Args.Verbose = value;
        }

        /// Network port number
        public int Port
        {
            get => Args.Port;
            set
            {
                if (!Net.IsValidPort(value))
                {
                    throw new ArgumentException("Invalid port", nameof(value));
                }
                Args.Port = value;
            }
        }

        /// Executable file path
        public string? ExePath
        {
            get => Args.ExePath;
            set => Args.ExePath = value;
        }

        /// Transfer file path
        public string? FilePath
        {
            get => Args.FilePath;
            set => Args.FilePath = value;
        }

        /// Network hostname
        public string? HostName
        {
            get => _hostName ?? (Address is null ? "" : Address.ToString());
            set => _hostName = value ?? (Address is null ? "" : Address.ToString());
        }

        /// IPv4 network address
        public IPAddress? Address
        {
            get => Args.Address;
            set => Args.Address = value ?? IPAddress.Any;
        }

        /// TCP socket client
        public TcpClient Client { get; set; }

        /// File transfer option
        protected bool Transfer => Args.TransOpt is not TransferOpt.None;

        /// Using an executable pipeline
        protected bool UsingExe => Args.UsingExe;

        /// Command-line arguments
        protected CmdLineArgs Args { get; set; }

        /// TCP network stream
        protected NetworkStream? NetStream { get; set; }

        /// <summary>
        ///  Initialize and run a new executable process on the local system.
        /// </summary>
        public bool StartProcess(string? exe)
        {
            (string? path, bool exists) = FileSys.ExistsOnPath(exe);

            if (!exists)
            {
                Dispose();
                Error.Handle(Except.ExePath, exe, true);
            }

            _process = new Process
            {
                StartInfo = Command.GetExeStartInfo(ExePath = path)
            };
            return _process.Start();
        }

        /// <summary>
        ///  Activate asynchronous communication between the source and
        ///  destination streams in each of the underlying pipelines.
        /// </summary>
        public virtual void Connect()
        {
            _ = NetStream ?? throw new ArgumentNullException(nameof(NetStream));

            if (!_validArgsCombos)
            {
                ValidateArgsCombinations();
            }

            AddPipes(Args.PipeVariant);
            _pipes?.ForEach(p => p?.Connect());
        }

        /// <summary>
        ///  Dispose of all unmanaged resources and handle the given error.
        /// </summary>
        public virtual void PipeError(Except type,
                                      HostEndPoint target,
                                      Exception? ex = default,
                                      Level level = default) {
            Dispose();
            Error.Handle(type, target.ToString(), ex, level);
        }

        /// <summary>
        ///  Dispose of all unmanaged resources and handle the given error.
        /// </summary>
        public virtual void PipeError(Except type,
                                      string? arg,
                                      Exception? ex = default,
                                      Level level = default) {
            Dispose();
            Error.Handle(type, arg, ex, level);
        }

        /// <summary>
        ///  Release all the underlying unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            _pipes?.ForEach(p => p?.Dispose());

            _process?.Dispose();
            _netReader?.Dispose();
            _netWriter?.Dispose();

            Client?.Close();
            NetStream?.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Validate the underlying command-line argument combinations.
        /// </summary>
        protected void ValidateArgsCombinations()
        {
            if (!_validArgsCombos)
            {
                // Combination: --exec, --output/--send
                if (UsingExe && Transfer)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--exec, --output/--send");
                }

                bool isTextPipe = !Args.Payload.IsNullOrEmpty();

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

                if (Parser.IndexOfFlag(Program.OrigArgs, "--zero-io") != -1)
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

                _validArgsCombos = true;
            }
        }

        /// <summary>
        ///  Initialize the underlying pipelines based on the given pipeline type.
        /// </summary>
        protected void AddPipes(PipeType pipeType)
        {
            if (NetStream is null || !NetStream.CanRead || !NetStream.CanWrite)
            {
                throw new ArgumentException(nameof(NetStream));
            }

            _netWriter = new StreamWriter(NetStream)
            {
                AutoFlush = true
            };
            _netReader = new StreamReader(NetStream);

            // Initialize the pipeline(s)
            _pipes = GetPipelines(pipeType);
        }

        /// <summary>
        ///  Wait for the underlying pipeline(s) to be disconnected or
        ///  the system command shell process to exit.
        /// </summary>
        protected void WaitForExit(int msPollDelay = 100)
        {
            while (Client.Connected)
            {
                Task.Delay(msPollDelay).Wait();

                if (ProcessExited() || !PipelinesConnected())
                {
                    break;
                }
            }
        }

        /// <summary>
        ///  Get a list of pipelines based on the given pipeline type.
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
                    pipes.AddRange(new[]
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
                    if (Args.TransOpt is TransferOpt.None)
                    {
                        throw new ArgumentException(nameof(Args.TransOpt));
                    }

                    if (Args.TransOpt is TransferOpt.Collect)
                    {
                        pipes.Add(new FilePipe(Args, _netReader));
                    }
                    else
                    {
                        pipes.Add(new FilePipe(Args, _netWriter));
                    }
                    break;
                }
                case PipeType.Process:  // Process pipelines
                {
                    pipes.AddRange(new[]
                    {
                        new ProcessPipe(Args, _netReader, _process?.StandardInput),
                        new ProcessPipe(Args, _process?.StandardOutput, _netWriter),
                        new ProcessPipe(Args, _process?.StandardError, _netWriter)
                    });
                    break;
                }
                case PipeType.Status:   // Zero-IO pipeline
                {
                    pipes.Add(new StatusPipe(Args, _netWriter));
                    break;
                }
                case PipeType.Text:     // Text pipeline
                {
                    pipes.Add(new TextPipe(Args, _netWriter));
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
        ///  Determine whether the underlying command shell process exited.
        /// </summary>
        private bool ProcessExited() => UsingExe && (_process?.HasExited ?? false);

        /// <summary>
        ///  Determine whether all the non-null underlying pipelines are connected.
        /// </summary>
        private bool PipelinesConnected()
        {
            bool connected = true;

            if (_pipes is not null)
            {
                int nullCount = _pipes.Where(p => p is null).Count();

                if (_pipes.Count > 0 && nullCount != _pipes.Count)
                {
                    foreach (Pipeline pipe in _pipes)
                    {
                        if (pipe is not null && !pipe.Connected)
                        {
                            connected = false;
                            break;
                        }
                    }
                }
            }
            return connected;
        }
    }
}
