using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional stream pipeline used to transfer arbitrary string data.
/// </summary>
internal class TextPipe : Pipeline, IConnectable
{
    private MemoryStream _memoryStream;  // Memory stream buffer

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public TextPipe(CmdLineArgs args, StreamWriter? dest) : base(args)
    {
        if (args.Payload.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(args));
        }

        _memoryStream = new MemoryStream();
        StatusMsg = "Payload successfully transmitted";

        Dest = dest ?? throw new ArgumentNullException(nameof(dest));
        Source = new StreamReader(_memoryStream);
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~TextPipe() => Dispose();

    /// String network payload
    protected string Payload
    {
        get => Args.Payload ?? string.Empty;
        set
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(value));
            }

            _memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            Args.Payload = value;
        }
    }

    /// Completion status message
    protected string StatusMsg { get; set; }

    /// <summary>
    ///  Release all the underlying unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        _memoryStream?.Dispose();
        base.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Asynchronously transfer the user-defined string payload
    ///  between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;

        StringBuilder data = new(await ReadToEndAsync());
        await WriteAsync(data, token);

        if (Args.Verbose)
        {
            Style.Output(StatusMsg);
        }

        Disconnect();
        Dispose();
    }
}
