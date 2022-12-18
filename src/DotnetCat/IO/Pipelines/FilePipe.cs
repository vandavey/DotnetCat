using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer file data.
/// </summary>
internal class FilePipe : SocketPipe, IErrorHandled
{
    private readonly TransferOpt _transfer;  // File transfer option

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, StreamReader? src) : base(args)
    {
        if (args.FilePath.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(args));
        }
        _transfer = TransferOpt.Collect;

        Source = src ?? throw new ArgumentNullException(nameof(src));

        Dest = new StreamWriter(CreateFile(FilePath))
        {
            AutoFlush = true
        };
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, StreamWriter? dest) : base(args)
    {
        if (args.FilePath.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(args));
        }
        _transfer = TransferOpt.Transmit;

        Dest = dest ?? throw new ArgumentNullException(nameof(dest));
        Source = new StreamReader(OpenFile(FilePath));
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~FilePipe() => Dispose();

    /// Enable verbose console output
    public bool Verbose => Args.Verbose;

    /// Source or destination path
    public string FilePath
    {
        get => Args.FilePath ??= string.Empty;
        set => Args.FilePath = value ?? string.Empty;
    }

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    public virtual void PipeError(Except type,
                                  string? arg,
                                  Exception? ex = default,
                                  Level level = default) {
        Dispose();
        Error.Handle(type, arg, ex, level);
    }

    /// <summary>
    ///  Create or overwrite a file at the given file path for writing.
    /// </summary>
    protected FileStream CreateFile(string path)
    {
        if (path.IsNullOrEmpty())
        {
            PipeError(Except.EmptyPath, "-o/--output");
        }
        DirectoryInfo? info = Directory.GetParent(path);

        if (!Directory.Exists(info?.FullName))
        {
            PipeError(Except.DirectoryPath, info?.FullName);
        }

        return new FileStream(path,
                              FileMode.Create,
                              FileAccess.Write,
                              FileShare.Write,
                              bufferSize: BUFFER_SIZE,
                              useAsync: true);
    }

    /// <summary>
    ///  Open an existing file at the given file path for reading.
    /// </summary>
    protected FileStream OpenFile(string? path)
    {
        if (path.IsNullOrEmpty())
        {
            PipeError(Except.EmptyPath, "-s/--send");
        }
        FileInfo info = new(path ?? string.Empty);

        if (!info.Exists)
        {
            PipeError(Except.FilePath, info.FullName);
        }

        return new FileStream(info.FullName,
                              FileMode.Open,
                              FileAccess.Read,
                              FileShare.Read,
                              bufferSize: 4096,
                              useAsync: true);
    }

    /// <summary>
    ///  Asynchronously perform the file transfer between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;
        StringBuilder data = new();

        // Print file transfer start message
        if (Verbose)
        {
            if (_transfer is TransferOpt.Transmit)
            {
                Style.Info($"Transmitting '{FilePath}' data...");
            }
            else
            {
                Style.Info($"Writing socket data to '{FilePath}'...");
            }
        }

        data.Append(await ReadToEndAsync());
        await WriteAsync(data, token);

        // Print file transfer complete message
        if (Verbose)
        {
            if (_transfer is TransferOpt.Transmit)
            {
                Style.Output("File successfully transmitted");
            }
            else
            {
                Style.Output("File download completed");
            }
        }

        Disconnect();
        Dispose();
    }
}
