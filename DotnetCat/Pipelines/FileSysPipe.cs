using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Base pipeline class for file system pipelines
    /// </summary>
    class FileSysPipe : StreamPipe, IConnectable
    {
        /// Initialize new FileSysPipe
        protected FileSysPipe() : base()
        {
            this.Error = new ErrorHandler();
        }

        public string FilePath { get; set; }

        public IOActionType IOAction { get; set; }

        public bool Recursive { get => Program.Recursive; }

        public bool Verbose { get => Program.Verbose; }

        protected ErrorHandler Error { get; }

        /// Activate communication between the pipe streams
        public override void Connect()
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }
            else if (Dest == null)
            {
                throw new ArgumentNullException(nameof(Dest));
            }

            CTS = new CancellationTokenSource();
            Worker = ConnectAsync(CTS.Token);
        }

        /// Determine file type from given file path
        protected FileType GetFileType(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                Error.Handle(ErrorType.FilePath, path);
            }

            FileType fileType = FileType.File;

            switch (fileInfo.Attributes)
            {
                case FileAttributes.Archive:
                    if (fileInfo.Extension != "")
                    {
                        fileType = FileType.Archive;
                    }
                    break;
                case FileAttributes.Directory:
                    fileType = FileType.Directory;
                    break;
                case FileAttributes.Encrypted:
                    fileType = FileType.Protected;
                    break;
                case FileAttributes.ReadOnly:
                    fileType = FileType.Protected;
                    break;
                case FileAttributes.ReparsePoint:
                    fileType = FileType.SymLink;
                    break;
                case FileAttributes.System:
                    fileType = FileType.Protected;
                    break;
                default:
                    break;
            }

            return fileType;
        }

        /// Create and open new file for writing
        protected static FileStream CreateFile(string path,
                                               ErrorHandler error) {
            if (string.IsNullOrEmpty(path))
            {
                error.Handle(ErrorType.EmptyPath, "-o/--output");
            }

            DirectoryInfo info = Directory.GetParent(path);

            if (!Directory.Exists(info.FullName))
            {
                error.Handle(ErrorType.DirectoryPath, info.FullName);
            }

            return new FileStream(
                path, FileMode.Open, FileAccess.Write,
                FileShare.Write, bufferSize: 1024, useAsync: true
            );
        }

        /// Open specified FileStream to read or write
        protected static FileStream OpenFile(string path,
                                             ErrorHandler error) {
            if (string.IsNullOrEmpty(path))
            {
                error.Handle(ErrorType.EmptyPath, "-s/--send");
            }

            FileInfo info = new FileInfo(path);

            if (!File.Exists(info.FullName))
            {
                error.Handle(ErrorType.FilePath, info.FullName);
            }

            return new FileStream(
                info.FullName, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 4096, useAsync: true
            );
        }

        /// Extract zip archive files to target directory
        protected FileStream UnpackZip(string zipPath, string dirPath)
        {
            // TODO: Unpack zip folder after read from socket
            throw new NotImplementedException();
        }

        /// Delete file at the specified filepath
        protected void DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                Error.Handle(ErrorType.FilePath, path);
            }

            File.Delete(path);
        }

        /// Activate async communication
        private async Task ConnectAsync(CancellationToken token)
        {
            StyleHandler style = new StyleHandler();
            StringBuilder data = new StringBuilder();

            Connected = true;

            if (Verbose)
            {
                if (IOAction == IOActionType.ReadFile)
                {
                    style.Status("Beginning file transmission...");
                }
                else
                {
                    style.Status($"Writing socket data to {FilePath}...");
                }
            }

            data.Append(await Source.ReadToEndAsync());

            await Dest.WriteAsync(data, token);
            await Dest.FlushAsync();

            if (Verbose)
            {
                if (IOAction == IOActionType.ReadFile)
                {
                    style.Status($"{FilePath} data successfully sent");
                }
                else
                {
                    style.Status($"Data successfully written to {FilePath}");
                }
            }

            Disconnect();
            Dispose();
        }
    }
}
