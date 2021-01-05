using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline class for normal file related data
    /// </summary>
    class FilePipe : StreamPipe, IConnectable
    {
        private readonly List<string> _zipExt;

        /// Initialize new object
        public FilePipe(StreamReader src, string path) : this()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }

            Source = src ?? throw new ArgNullException(nameof(src));
            FilePath = path;

            PathType = GetFileType(path);
            Dest = new StreamWriter(CreateFile(path, Error));
        }

        /// Initialize new object
        public FilePipe(string path, StreamWriter dest) : this()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }

            Dest = dest ?? throw new ArgNullException(nameof(dest));
            FilePath = path;

            PathType = GetFileType(path);
            Source = new StreamReader(OpenFile(path, Error));
        }

        /// Initialize new object
        protected FilePipe() : base()
        {
            _zipExt = new List<string>
            {
                "zip", "tar", "gz", "7z"
            };
            IOAction = Communicate.Collect;
            Error = new ErrorHandler();
        }

        public bool Verbose => Program.Verbose;

        public bool Recursive => Program.Recursive;

        public string FilePath { get; set; }

        public Communicate IOAction { get; set; }

        protected ErrorHandler Error { get; }

        protected bool FileFound { get; set; }

        protected FileType PathType { get; set; }

        /// Activate communication between the pipe streams
        public override void Connect()
        {
            _ = Source ?? throw new ArgNullException(nameof(Source));
            _ = Dest ?? throw new ArgNullException(nameof(Dest));

            CTS = new CancellationTokenSource();
            Worker = ConnectAsync(CTS.Token);
        }

        /// Release any unmanaged resources
        public override void Dispose()
        {
            // TODO: unpack zip if *.~dncat.zip detected?
            base.Dispose();
        }

        /// Determine file type from given file path
        public FileType GetFileType(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }
            FileInfo fileInfo = new FileInfo(path);

            // Check whether file is normal/archive
            if (fileInfo.Attributes is FileAttributes.Archive)
            {
                if (!_zipExt.Contains(fileInfo.Extension))
                {
                    fileInfo.Attributes = FileAttributes.Normal;
                }
            }

            // Determine file type from attributes
            return fileInfo.Attributes switch
            {
                FileAttributes.Archive => FileType.Archive,
                FileAttributes.Device => FileType.Device,
                FileAttributes.Directory => FileType.Directory,
                FileAttributes.Encrypted => FileType.Protected,
                FileAttributes.Normal => FileType.File,
                FileAttributes.NotContentIndexed => FileType.Protected,
                FileAttributes.System => FileType.Protected,
                _ => FileType.None,
            };
        }

        /// Create and open new file for writing
        protected FileStream CreateFile(string path, ErrorHandler error)
        {
            if (string.IsNullOrEmpty(path))
            {
                error.Handle(Except.EmptyPath, "-o/--output");
            }
            DirectoryInfo info = Directory.GetParent(path);

            // Directory does not exist
            if (!Directory.Exists(info.FullName))
            {
                error.Handle(Except.DirectoryPath, info.FullName);
            }

            return new FileStream(path, FileMode.Open,
                                        FileAccess.Write,
                                        FileShare.Write,
                                        bufferSize: 1024,
                                        useAsync: true);
        }

        /// Open specified FileStream to read or write
        protected FileStream OpenFile(string path, ErrorHandler error)
        {
            if (string.IsNullOrEmpty(path))
            {
                error.Handle(Except.EmptyPath, "-s/--send");
            }
            FileSystemInfo info = new FileInfo(path);

            // Specified file does not exist
            if (!info.Exists)
            {
                error.Handle(Except.FilePath, info.FullName);
            }

            return new FileStream(info.FullName, FileMode.Open,
                                                 FileAccess.Read,
                                                 FileShare.Read,
                                                 bufferSize: 4096,
                                                 useAsync: true);
        }

        /// Activate async network communication
        private async Task ConnectAsync(CancellationToken token)
        {
            StyleHandler style = new StyleHandler();
            StringBuilder data = new StringBuilder();

            Connected = true;

            // Print connection started info
            if (Verbose)
            {
                if (IOAction is Communicate.Transmit)
                {
                    style.Status("Beginning file transmission...");
                }
                else
                {
                    style.Status($"Writing socket data to '{FilePath}'...");
                }
            }
            data.Append(await Source.ReadToEndAsync());

            await Dest.WriteAsync(data, token);
            await Dest.FlushAsync();

            // Print connection completed info
            if (Verbose)
            {
                if (IOAction is Communicate.Transmit)
                {
                    style.Status($"'{FilePath}' contents successfully sent");
                }
                else
                {
                    style.Status($"Data successfully written to '{FilePath}'");
                }
            }

            Disconnect();
            Dispose();
        }
    }
}
