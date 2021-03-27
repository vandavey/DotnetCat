using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using ArgNullException = System.ArgumentNullException;
using Style = DotnetCat.Handlers.StyleHandler;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline class for normal file related data
    /// </summary>
    class FilePipe : StreamPipe, IErrorHandled
    {
        private readonly TransferOpt _transfer;

        /// Initialize object
        public FilePipe(StreamReader src, string path) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }
            _transfer = TransferOpt.Collect;

            Source = src ?? throw new ArgNullException(nameof(src));
            FilePath = path;

            Dest = new StreamWriter(CreateFile(FilePath))
            {
                AutoFlush = true
            };
        }

        /// Initialize object
        public FilePipe(string path, StreamWriter dest) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgNullException(nameof(path));
            }
            _transfer = TransferOpt.Transmit;

            Dest = dest ?? throw new ArgNullException(nameof(dest));
            Source = new StreamReader(OpenFile(FilePath = path));
        }

        /// Cleanup resources
        ~FilePipe() => Dispose();

        public static bool Verbose => Program.Verbose;

        public string FilePath { get; set; }

        /// Dispose of unmanaged resources and handle error
        public virtual void PipeError(Except type, string arg,
                                                   Exception ex = null,
                                                   Level level = Level.Error) {
            Dispose();
            ErrorHandler.Handle(type, arg, ex, level);
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

            return new FileStream(path, FileMode.Open, FileAccess.Write,
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
        protected override async Task ConnectAsync(CancellationToken token)
        {
            Connected = true;
            StringBuilder data = new();

            // Print connection started info
            if (Verbose)
            {
                if (_transfer is TransferOpt.Transmit)
                {
                    Style.Info($"Transmitting '{FilePath}'...");
                }
                else
                {
                    Style.Info($"Writing socket data to '{FilePath}'...");
                }
            }

            data.Append(await Source.ReadToEndAsync());
            await Dest.WriteAsync(data, token);

            // Print connection completed info
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
}
