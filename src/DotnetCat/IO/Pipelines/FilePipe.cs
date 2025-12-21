using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer file data.
/// </summary>
internal class FilePipe : SocketPipe
{
    private readonly TransferOpt _transfer;  // File transfer option

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, [NotNull] StreamReader? src) : base(args)
    {
        ThrowIf.NullOrEmpty(args.FilePath);
        _transfer = TransferOpt.Collect;

        Source = ThrowIf.Null(src);
        Dest = new StreamWriter(CreateFile(FilePath)) { AutoFlush = true };
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, [NotNull] StreamWriter? dest) : base(args)
    {
        ThrowIf.NullOrEmpty(args.FilePath);
        _transfer = TransferOpt.Transmit;

        Dest = ThrowIf.Null(dest);
        Source = new StreamReader(OpenFile(FilePath));
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~FilePipe() => Dispose(false);

    /// <summary>
    ///  Source or destination path.
    /// </summary>
    public string FilePath
    {
        get => Args.FilePath ??= string.Empty;
        set => Args.FilePath = value ?? string.Empty;
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
        string? parentPath = FileSys.ParentPath(path);

        if (!FileSys.DirectoryExists(parentPath))
        {
            PipeError(Except.DirectoryPath, parentPath);
        }

        return new FileStream(path,
                              FileMode.Create,
                              FileAccess.Write,
                              FileShare.Write,
                              READ_BUFFER_SIZE,
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
                              WRITE_BUFFER_SIZE,
                              useAsync: true);
    }

    /// <summary>
    ///  Asynchronously perform the file transfer between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;
        StringBuilder data = new();

        if (_transfer is TransferOpt.Collect)
        {
            Output.Log($"Downloading socket data to '{FilePath}'...");
        }
        else if (_transfer is TransferOpt.Transmit)
        {
            Output.Log($"Transmitting '{FilePath}' data...");
        }

        data.Append(await ReadToEndAsync());
        await WriteAsync(data, token);

        if (_transfer is TransferOpt.Collect)
        {
            Output.Status($"File successfully downloaded to '{FilePath}'");
        }
        else if (_transfer is TransferOpt.Transmit)
        {
            Output.Status("File successfully transmitted");
        }

        Disconnect();
        Dispose();
    }
}
