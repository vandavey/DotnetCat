using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to perform network connection testing.
/// </summary>
internal sealed class StatusPipe : TextPipe
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StatusPipe(CmdLineArgs args, [NotNull] StreamWriter? dest) : base(args, dest)
    {
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~StatusPipe() => Dispose(false);

    /// <summary>
    ///  Asynchronously transfer an empty string between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;

        StringBuilder data = new(await ReadToEndAsync());
        await WriteAsync(data, token);

        Output.Status($"Connection accepted by {Program.SocketNode?.Endpoint}");

        Disconnect();
        Dispose();
    }
}
