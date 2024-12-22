using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transmit arbitrary string data.
/// </summary>
internal class TextPipe : SocketPipe
{
    private bool _disposed;              // Object disposed

    private MemoryStream _memoryStream;  // Memory stream buffer

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public TextPipe(CmdLineArgs args, [NotNull] StreamWriter? dest) : base(args)
    {
        ThrowIf.Null(dest);

        _disposed = false;
        _memoryStream = new MemoryStream();

        Dest = dest;
        Source = new StreamReader(_memoryStream);
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~TextPipe() => Dispose(false);

    /// <summary>
    ///  String network payload.
    /// </summary>
    protected string Payload
    {
        get => Args.Payload ?? string.Empty;
        set
        {
            ThrowIf.NullOrEmpty(value);
            _memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            Args.Payload = value;
        }
    }

    /// <summary>
    ///  Free the underlying resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _memoryStream.Dispose();
            }
            _disposed = true;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///  Asynchronously transfer the user-defined
    ///  string payload between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;

        StringBuilder data = new(await ReadToEndAsync());
        await WriteAsync(data, token);

        if (Args.Verbose)
        {
            Output.Status("Payload successfully transmitted");
        }

        Disconnect();
        Dispose();
    }
}
