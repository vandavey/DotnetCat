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
    class FilePipe : StreamPipe, IErrorHandled
    {
        private readonly TransferOpt _transfer;

        private readonly List<string> _zipExt;

        /// Initialize new object
        public FilePipe(StreamReader src, string path) : this()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }
            _transfer = TransferOpt.Collect;

            Source = src ?? throw new ArgNullException(nameof(src));
            FilePath = path;

            PathType = GetFileType(path);
            Dest = new StreamWriter(CreateFile(path));
        }

        /// Initialize new object
        public FilePipe(string path, StreamWriter dest) : this()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }
            _transfer = TransferOpt.Transmit;

            Dest = dest ?? throw new ArgNullException(nameof(dest));
            FilePath = path;

            PathType = GetFileType(path);
            Source = new StreamReader(OpenFile(path));
        }

        /// Initialize new object
        protected FilePipe() : base()
        {
            _transfer = TransferOpt.None;

            _zipExt = new List<string>
            {
                "zip", "tar", "gz", "7z"
            };
            Error = new ErrorHandler();
        }

        public bool Verbose => Program.Verbose;

        /** 
        * TODO: implement recursive functionality
        **/
        public bool Recursive => Program.Recursive;

        public string FilePath { get; set; }

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

        /// Dispose of unmanaged resources and handle error
        public virtual void PipeError(Except type, string arg,
                                                   Exception ex = null) {
            Dispose();
            Error.Handle(type, arg, ex);
        }

        /// Release any unmanaged resources
        public override void Dispose()
        {
            /**
            * TODO: unpack zip if *.~dncat.zip detected
            **/
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
        protected FileStream CreateFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                PipeError(Except.EmptyPath, "-o/--output");
            }
            DirectoryInfo info = Directory.GetParent(path);

            // Directory does not exist
            if (!Directory.Exists(info.FullName))
            {
                PipeError(Except.DirectoryPath, info.FullName);
            }

            return new FileStream(path, FileMode.Open,
                                        FileAccess.Write,
                                        FileShare.Write,
                                        bufferSize: 1024,
                                        useAsync: true);
        }

        /// Open specified FileStream to read or write
        protected FileStream OpenFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                PipeError(Except.EmptyPath, "-s/--send");
            }
            FileSystemInfo info = new FileInfo(path);

            // Specified file does not exist
            if (!info.Exists)
            {
                PipeError(Except.FilePath, info.FullName);
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
                if (_transfer is TransferOpt.Transmit)
                {
                    style.Status($"Transmitting '{FilePath}'...");
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
                if (_transfer is TransferOpt.Transmit)
                {
                    style.Status($"File successfully transmitted");
                }
                else
                {
                    style.Status($"File download completed");
                }
            }

            Disconnect();
            Dispose();
        }
    }
}
