using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to perform network connection testing.
/// </summary>
internal class StatusPipe : TextPipe
{
    private readonly string _statusMsg;  // Completion status message

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StatusPipe(CmdLineArgs args, [NotNull] StreamWriter? dest) : base(args, dest)
    {
        ThrowIf.Null(Program.SockNode);

        string target = $"{Program.SockNode.HostName}:{Program.SockNode.Port}";
        _statusMsg = $"Connection accepted by {target}";
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

        Output.Status(_statusMsg);

        Disconnect();
        Dispose();
    }
}
