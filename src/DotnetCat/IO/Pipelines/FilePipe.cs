using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.Utils;
using static DotnetCat.IO.Constants;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer file data.
/// </summary>
internal sealed class FilePipe : SocketPipe
{
    private readonly TransferOpt _transfer;  // File transfer option

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, [NotNull] StreamWriter? dest) : base(args)
    {
        _transfer = TransferOpt.Transmit;

        Dest = ThrowIf.Null(dest);
        Source = new StreamReader(OpenReadStream(FilePath));
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, [NotNull] StreamReader? src) : base(args)
    {
        _transfer = TransferOpt.Collect;

        Source = ThrowIf.Null(src);
        Dest = new StreamWriter(OpenWriteStream(FilePath)) { AutoFlush = true };
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
    ///  Asynchronously perform the file transfer between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;
        StringBuilder data = new();

        if (ThrowIf.UndefinedOrDefault(_transfer) is TransferOpt.Collect)
        {
            Output.Log($"Downloading file to '{FilePath}'...");
        }
        else if (_transfer is TransferOpt.Transmit)
        {
            Output.Log($"Transmitting file '{FilePath}'...");
        }

        data.Append(await ReadToEndAsync());
        await WriteAsync(data, token);

        if (_transfer is TransferOpt.Collect)
        {
            Output.Status($"File successfully downloaded to '{FilePath}'");
        }
        else if (_transfer is TransferOpt.Transmit)
        {
            Output.Status($"Successfully transmitted '{FilePath}'");
        }

        Disconnect();
        Dispose();
    }

    /// <summary>
    ///  Get the file stream buffer size to use for the given transfer option.
    /// </summary>
    private static int BufferSize(TransferOpt transfer)
    {
        ThrowIf.UndefinedOrDefault(transfer);
        return transfer is TransferOpt.Transmit ? READ_BUFFER_SIZE : WRITE_BUFFER_SIZE;
    }

    /// <summary>
    ///  Open a stream to read a file at the given file path.
    /// </summary>
    private FileStream OpenReadStream([NotNull] string? path)
    {
        if (path.IsNullOrEmpty())
        {
            PipeError(Except.EmptyPath, "-s/--send");
        }
        FileInfo info = new(path);

        if (!info.Exists)
        {
            PipeError(Except.FilePath, info.FullName);
        }
        return MakeStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    ///  Open a stream to create or overwrite a file at the given file path.
    /// </summary>
    private FileStream OpenWriteStream([NotNull] string? path)
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
        return MakeStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
    }

    /// <summary>
    ///  Open a file stream to read or write to a new or existing file.
    /// </summary>
    private FileStream MakeStream(string path,
                                  FileMode mode,
                                  FileAccess access,
                                  FileShare share)
    {
        return new FileStream(path,
                              ThrowIf.Undefined(mode),
                              ThrowIf.Undefined(access),
                              ThrowIf.Undefined(share),
                              BufferSize(_transfer),
                              useAsync: true);
    }
}
