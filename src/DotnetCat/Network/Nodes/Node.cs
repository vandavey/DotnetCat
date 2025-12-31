using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Shell;
using DotnetCat.Utils;
using static DotnetCat.Network.Constants;

namespace DotnetCat.Network.Nodes;

/// <summary>
///  Abstract TCP network socket node. This is the base class
///  for all socket nodes in the <see cref="Nodes"/> namespace.
/// </summary>
internal abstract class Node : IConnectable
{
    private readonly List<SocketPipe> _pipes;  // TCP socket pipelines

    private bool _disposed;                    // Object disposed
    private bool _validArgsCombos;             // Valid command-line arguments

    private Process? _process;                 // Executable process

    private StreamReader? _netReader;          // TCP stream reader

    private StreamWriter? _netWriter;          // TCP stream writer

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected Node(CmdLineArgs args)
    {
        _pipes = [];
        _disposed = _validArgsCombos = false;

        Args = args;
        Endpoint = new HostEndPoint(Args.HostName, Args.Address, Args.Port);
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~Node() => Dispose(false);

    /// <summary>
    ///  Executable file path.
    /// </summary>
    public string? ExePath
    {
        get => Args.ExePath;
        private set => Args.ExePath = value;
    }

    /// <summary>
    ///  Host endpoint to use for connection.
    /// </summary>
    public HostEndPoint Endpoint { get; }

    /// <summary>
    ///  TCP socket client.
    /// </summary>
    public Socket? Socket { get; protected set; }

    /// <summary>
    ///  File transfer option is set.
    /// </summary>
    protected bool Transfer => Args.TransOpt is not TransferOpt.None;

    /// <summary>
    ///  Using an executable pipeline.
    /// </summary>
    protected bool UsingExe => Args.UsingExe;

    /// <summary>
    ///  Command-line arguments.
    /// </summary>
    protected CmdLineArgs Args { get; private set; }

    /// <summary>
    ///  TCP network stream.
    /// </summary>
    protected NetworkStream? NetStream { get; set; }

    /// <summary>
    ///  Initialize a client or server node from the given command-line arguments.
    /// </summary>
    public static Node Make(CmdLineArgs args)
    {
        return args.Listen ? new ServerNode(args) : new ClientNode(args);
    }

    /// <summary>
    ///  Activate asynchronous communication between the source and
    ///  destination streams in each of the underlying pipelines.
    /// </summary>
    public virtual void Connect()
    {
        ThrowIf.Null(NetStream);
        ValidateArgsCombinations();

        AddPipes(Args.PipeVariant);
        _pipes.ForEach(p => p?.Connect());
    }

    /// <summary>
    ///  Free the underlying resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    public void PipeError(Except type, [NotNull] string? arg, Exception? ex = default)
    {
        Dispose();
        Error.Handle(type, arg, ex);
    }

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    public void PipeError(Except type, HostEndPoint target, Exception? ex = default)
    {
        PipeError(type, target.ToString(), ex);
    }

    /// <summary>
    ///  Initialize and execute the given executable on the local system.
    /// </summary>
    public bool StartProcess([NotNull] string? exe)
    {
        if (!FileSys.ExistsOnPath(exe, out string? path))
        {
            Dispose();
            Error.Handle(Except.ExePath, exe, true);
        }
        ExePath = path;

        _process = new Process
        {
            StartInfo = Command.ExeStartInfo(ExePath)
        };
        return _process.Start();
    }

    /// <summary>
    ///  Free the underlying resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _pipes?.Dispose();
                _process?.Dispose();
                _netReader?.Dispose();
                _netWriter?.Dispose();

                Socket?.Dispose();
                NetStream?.Dispose();
            }
            _disposed = true;
        }
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

            if (Args.PipeVariant is PipeType.Status)
            {
                // Combination: --listen, --zero-io
                if (this is ServerNode)
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
            throw new InvalidOperationException("Invalid network stream state.");
        }

        _netReader = new StreamReader(NetStream);
        _netWriter = new StreamWriter(NetStream) { AutoFlush = true };

        _pipes.AddRange(MakePipes(pipeType));
    }

    /// <summary>
    ///  Wait for the underlying pipeline(s) to be disconnected
    ///  or the system command shell process to exit.
    /// </summary>
    protected void WaitForExit(int pollInterval = POLL_INTERVAL)
    {
        WaitForExitAsync(pollInterval).AwaitResult();
    }

    /// <summary>
    ///  Asynchronously wait for the underlying pipeline(s) to be
    ///  disconnected or the system command shell process to exit.
    /// </summary>
    protected async Task WaitForExitAsync(int pollInterval = POLL_INTERVAL)
    {
        ThrowIf.Null(Socket);

        while (Socket.Connected)
        {
            await Task.Delay(pollInterval);

            if (ProcessExited() || !PipelinesConnected())
            {
                break;
            }
        }
    }

    /// <summary>
    ///  Initialize a list of pipelines from the given pipeline type.
    /// </summary>
    private List<SocketPipe> MakePipes(PipeType type)
    {
        List<SocketPipe> pipelines = [];

        switch (ThrowIf.Undefined(type))
        {
            case PipeType.Stream:
                pipelines.AddRange(MakeStreamPipes);
                break;
            case PipeType.File:
                pipelines.Add(MakeFilePipe);
                break;
            case PipeType.Process:
                pipelines.AddRange(MakeProcessPipes);
                break;
            case PipeType.Status:
                pipelines.Add(new StatusPipe(Args, _netWriter));
                break;
            case PipeType.Text:
                pipelines.Add(new TextPipe(Args, _netWriter));
                break;
            default:
                break;
        }
        return pipelines;
    }

    /// <summary>
    ///  Initialize an array of console stream pipelines.
    /// </summary>
    private StreamPipe[] MakeStreamPipes() =>
    [
        new StreamPipe(_netReader, new StreamWriter(Console.OpenStandardOutput())
        {
            AutoFlush = true
        }),
        new StreamPipe(new StreamReader(Console.OpenStandardInput()), _netWriter)
    ];

    /// <summary>
    ///  Initialize an array of executable process pipelines.
    /// </summary>
    private ProcessPipe[] MakeProcessPipes() =>
    [
        new ProcessPipe(Args, _netReader, _process?.StandardInput),
        new ProcessPipe(Args, _process?.StandardOutput, _netWriter),
        new ProcessPipe(Args, _process?.StandardError, _netWriter)
    ];

    /// <summary>
    ///  Initialize a file pipeline.
    /// </summary>
    private FilePipe MakeFilePipe()
    {
        FilePipe filePipe;

        // Pipe data from socket to file
        if (ThrowIf.Default(Args.TransOpt) is TransferOpt.Collect)
        {
            filePipe = new FilePipe(Args, _netReader);
        }
        else  // Pipe data from file to socket
        {
            filePipe = new FilePipe(Args, _netWriter);
        }
        return filePipe;
    }

    /// <summary>
    ///  Determine whether the underlying command shell process exited.
    /// </summary>
    private bool ProcessExited() => UsingExe && (_process?.HasExited ?? false);

    /// <summary>
    ///  Determine whether any of the non-null underlying pipelines are connected.
    /// </summary>
    private bool PipelinesConnected() => _pipes.Any(p => p?.Connected ?? false);
}
