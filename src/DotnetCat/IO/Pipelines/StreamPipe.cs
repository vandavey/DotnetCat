using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Shell;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer standard console stream data.
/// </summary>
internal sealed class StreamPipe : SocketPipe
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StreamPipe([NotNull] StreamReader? src, [NotNull] StreamWriter? dest) : base()
    {
        Source = ThrowIf.Null(src);
        Dest = ThrowIf.Null(dest);
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~StreamPipe() => Dispose(false);

    /// <summary>
    ///  Asynchronously transfer console stream data between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        StringBuilder data = new();

        int charsRead;
        Connected = true;

        while (Socket?.Connected ?? false)
        {
            if (token.IsCancellationRequested)
            {
                Disconnect();
                break;
            }

            charsRead = await ReadAsync(token);
            data.Append(Buffer.ToArray(), 0, charsRead);

            if (!Socket.Connected || charsRead <= 0)
            {
                Disconnect();
                break;
            }
            data = data.ReplaceLineEndings();

            // Clear the console screen buffer
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
