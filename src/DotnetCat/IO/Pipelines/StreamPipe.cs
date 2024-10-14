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
internal class StreamPipe : SocketPipe
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StreamPipe([NotNull] StreamReader? src, [NotNull] StreamWriter? dest) : base()
    {
        ThrowIf.Null(src);
        ThrowIf.Null(dest);

        Source = src;
        Dest = dest;
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~StreamPipe() => Dispose();

    /// <summary>
    ///  Asynchronously transfer console stream data between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
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

                if (!Client.Connected || charsRead <= 0)
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
}
