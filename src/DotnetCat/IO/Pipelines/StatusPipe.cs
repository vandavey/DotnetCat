using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Network;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to perform network connection testing.
/// </summary>
internal sealed class StatusPipe : TextPipe
{
    private readonly HostEndPoint _target;  // Remote host endpoint

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StatusPipe([NotNull] Socket? socket,
                      CmdLineArgs args,
                      [NotNull] StreamWriter? dest,
                      HostEndPoint target)
        : base(socket, args, dest)
    {
        _target = target;
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

        Output.Status($"Connection accepted by {_target}");

        Disconnect();
        Dispose();
    }
}
