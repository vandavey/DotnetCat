using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Network;
using DotnetCat.Shell;
using DotnetCat.Utils;
using static DotnetCat.IO.Constants;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Abstract unidirectional socket pipeline. This is the base class
///  for all socket pipelines in the <see cref="Pipelines"/> namespace.
/// </summary>
internal abstract class SocketPipe : IConnectable
{
    private bool _disposed;                      // Object disposed

    private CancellationTokenSource? _tokenSrc;  // Cancellation token source

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected SocketPipe()
    {
        _disposed = false;
        Connected = false;

        Args = new CmdLineArgs();
        NewLine = new StringBuilder(SysInfo.Eol);
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected SocketPipe(CmdLineArgs args) : this() => Args = args;

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~SocketPipe() => Dispose(false);

    /// <summary>
    ///  Underlying streams are connected.
    /// </summary>
    public bool Connected { get; protected set; }

    /// <summary>
    ///  TCP client is connected.
    /// </summary>
    protected static bool SocketConnected => Socket?.Connected ?? false;

    /// <summary>
    ///  TCP connection socket.
    /// </summary>
    protected static Socket? Socket => Program.SocketNode?.Socket;

    /// <summary>
    ///  Platform based EOL control sequence.
    /// </summary>
    protected StringBuilder NewLine { get; }

    /// <summary>
    ///  Character memory buffer.
    /// </summary>
    protected Memory<char> Buffer { get; set; }

    /// <summary>
    ///  Command-line arguments.
    /// </summary>
    protected CmdLineArgs Args { get; set; }

    /// <summary>
    ///  Pipeline data source.
    /// </summary>
    protected StreamReader? Source { get; set; }

    /// <summary>
    ///  Pipeline data destination.
    /// </summary>
    protected StreamWriter? Dest { get; set; }

    /// <summary>
    ///  Pipeline data transfer task.
    /// </summary>
    protected Task? Worker { get; set; }

    /// <summary>
    ///  Activate communication between the underlying streams.
    /// </summary>
    public void Connect()
    {
        ThrowIf.Null(Source);
        ThrowIf.Null(Dest);

        _tokenSrc = new CancellationTokenSource();
        Buffer = new Memory<char>(new char[READ_BUFFER_SIZE]);

        Worker = ConnectAsync(_tokenSrc.Token);
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
    ///  Free the underlying resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _tokenSrc?.Dispose();
                Source?.Dispose();
                Dest?.Dispose();
                Socket?.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    ///  Cancel communication between the underlying streams.
    /// </summary>
    protected void Disconnect()
    {
        Connected = false;
        _tokenSrc?.Cancel();
    }

    /// <summary>
    ///  Asynchronously transfer data between the underlying streams.
    /// </summary>
    protected abstract Task ConnectAsync(CancellationToken token);

    /// <summary>
    ///  Asynchronously read data from the underlying source stream
    ///  and write it to the underlying memory buffer.
    /// </summary>
    protected virtual async ValueTask<int> ReadAsync(CancellationToken token)
    {
        int bytesRead = -1;

        if (Source is not null && SocketConnected)
        {
            bytesRead = await Source.ReadAsync(Buffer, token);
        }
        return bytesRead;
    }

    /// <summary>
    ///  Asynchronously read all the data that is currently available
    ///  in the underlying source stream.
    /// </summary>
    protected virtual async ValueTask<string> ReadToEndAsync()
    {
        string buffer = string.Empty;

        if (Source is not null && SocketConnected)
        {
            buffer = await Source.ReadToEndAsync();
        }
        return buffer;
    }

    /// <summary>
    ///  Asynchronously write all the given data to the underlying destination stream.
    /// </summary>
    protected virtual async Task WriteAsync(StringBuilder data, CancellationToken token)
    {
        if (Dest is not null && SocketConnected)
        {
            await Dest.WriteAsync(data, token);
        }
    }
}
