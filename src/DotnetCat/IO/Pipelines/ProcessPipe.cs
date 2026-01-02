using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer executable process data.
/// </summary>
internal sealed class ProcessPipe : SocketPipe
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ProcessPipe([NotNull] Socket? socket,
                       CmdLineArgs args,
                       [NotNull] StreamReader? src,
                       [NotNull] StreamWriter? dest)
        : base(socket, args)
    {
        Source = ThrowIf.Null(src);
        Dest = ThrowIf.Null(dest);
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~ProcessPipe() => Dispose(false);

    /// <summary>
    ///  Asynchronously transfer the executable process data
    ///  between the underlying streams.
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

            await WriteAsync(data.ReplaceLineEndings(), token);
            data.Clear();
        }

        if (!Args.UsingExe)
        {
            Console.WriteLine();
        }
        Dispose();
    }
}
