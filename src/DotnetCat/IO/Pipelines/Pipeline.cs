using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Shell.Commands;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Abstract unidirectional stream pipeline. This is the base
///  class for all pipelines in the IO.Pipelines namespace.
/// </summary>
internal abstract class Pipeline : IConnectable
{
    protected const int BUFFER_SIZE = 1024;  // Memory buffer size

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected Pipeline()
    {
        Connected = false;

        Args = new CmdLineArgs();
        NewLine = new StringBuilder(Environment.NewLine);
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected Pipeline(CmdLineArgs args) : this() => Args = args;

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~Pipeline() => Dispose();

    /// Underlying streams are connected
    public bool Connected { get; protected set; }

    /// Operating system
    protected static Platform OS => Program.OS;

    /// TCP socket client
    protected static TcpClient? Client => Program.SockNode?.Client;

    /// TCP client is connected
    protected static bool ClientConnected => Client?.Connected ?? false;

    /// Platform based EOL escape sequence
    protected StringBuilder NewLine { get; }

    /// Pipeline cancellation token source
    protected CancellationTokenSource? CTS { get; set; }

    /// Character memory buffer
    protected Memory<char> Buffer { get; set; }

    /// Command-line arguments
    protected CmdLineArgs Args { get; set; }

    /// Pipeline data source
    protected StreamReader? Source { get; set; }

    /// Pipeline data destination
    protected StreamWriter? Dest { get; set; }

    /// Pipeline data transfer task
    protected Task? Worker { get; set; }

    /// <summary>
    ///  Activate communication between the underlying streams.
    /// </summary>
    public virtual void Connect()
    {
        _ = Source ?? throw new InvalidOperationException(nameof(Source));
        _ = Dest ?? throw new InvalidOperationException(nameof(Dest));

        CTS = new CancellationTokenSource();
        Buffer = new Memory<char>(new char[BUFFER_SIZE]);

        Worker = ConnectAsync(CTS.Token);
    }

    /// <summary>
    ///  Cancel communication between the underlying streams.
    /// </summary>
    public virtual void Disconnect()
    {
        Connected = false;
        CTS?.Cancel();
    }

    /// <summary>
    ///  Release all the underlying unmanaged resources.
    /// <summary>
    public virtual void Dispose()
    {
        Source?.Dispose();
        Dest?.Dispose();

        CTS?.Dispose();
        Client?.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Asynchronously transfer data between the underlying streams.
    /// </summary>
    protected virtual async Task ConnectAsync(CancellationToken token)
    {
        StringBuilder data = new();

        int charsRead;
        Connected = true;

        if (Client is not null)
        {
            while (Client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                charsRead = await ReadAsync(token);
                data.Append(Buffer.ToArray(), 0, charsRead);

                // Socket client was disconnected
                if (!Client.Connected || charsRead <= 0)
                {
                    Disconnect();
                    break;
                }
                data = FixLineEndings(data);

                // Clear console screen buffer
                if (Command.IsClearCmd(data.ToString()))
                {
                    Sequence.ClearScreen();
                    await WriteAsync(NewLine, token);
                }
                else  // Send the command
                {
                    await WriteAsync(data, token);
                }
                data.Clear();
            }
            Dispose();
        }
    }

    /// <summary>
    ///  Normalize line-endings based on the local operating system
    ///  so shell commands are properly interpreted.
    /// </summary>
    protected static StringBuilder FixLineEndings(StringBuilder data)
    {
        return OS is Platform.Win ? data : data.Replace("\r\n", "\n");
    }

    /// <summary>
    ///  Asynchronously read data from the underlying source stream
    ///  and write it to the underlying memory buffer.
    /// </summary>
    protected virtual async ValueTask<int> ReadAsync(CancellationToken token)
    {
        int bytesRead = -1;

        if (Source is not null && ClientConnected)
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

        if (Source is not null && ClientConnected)
        {
            buffer = await Source.ReadToEndAsync();
        }
        return buffer;
    }

    /// <summary>
    ///  Asynchronously write all the given data to the
    ///  underlying destination stream.
    /// </summary>
    protected virtual async Task WriteAsync(StringBuilder data,
                                            CancellationToken token) {

        if (Dest is not null && ClientConnected)
        {
            await Dest.WriteAsync(data, token);
        }
    }
}
