using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Handlers;
using DotnetCat.Utils;

namespace DotnetCat.Pipes
{
    enum IOAction { None, Output, Transmit }

    /// <summary>
    /// Handle file data communication operations
    /// </summary>
    class FilePipe : StreamPipe, IConnectable
    {
        private readonly ErrorHandler _error;

        /// Initialize new FilePipe
        public FilePipe(StreamReader src, string path) : base()
        {
            _error = new ErrorHandler();

            this.Source = src ?? throw new ArgumentNullException("src");
            this.FilePath = path ?? throw new ArgumentNullException("path");

            this.IOActionType = IOAction.Output;
            this.Dest = new StreamWriter(CreateFile(path, _error));
        }

        /// Initialize new FilePipe
        public FilePipe(string path, StreamWriter dest) : base()
        {
            _error = new ErrorHandler();

            this.FilePath = path ?? throw new ArgumentNullException("path");
            this.Dest = dest ?? throw new ArgumentNullException("dest");

            this.IOActionType = IOAction.Transmit;
            this.Source = new StreamReader(OpenFile(path, _error));
        }

        public string FilePath { get; }

        public IOAction IOActionType { get; }

        public bool Verbose { get => Program.IsVerbose; }

        /// Activate communication between the pipe streams
        public override void Connect()
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }
            else if (Dest == null)
            {
                throw new ArgumentNullException("Dest");
            }

            CTS = new CancellationTokenSource();
            Worker = ConnectAsync(CTS.Token);
        }

        /// Create and open new file for writing
        private static FileStream CreateFile(string path, ErrorHandler error)
        {
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
                path, FileMode.Create, FileAccess.Write,
                FileShare.Write, bufferSize: 1024, useAsync: true
            );
        }

        /// Open specified FileStream for reading/writing
        private static FileStream OpenFile(string path, ErrorHandler error)
        {
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
                path, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 4096, useAsync: true
            );
        }

        /// Activate async communication
        private async Task ConnectAsync(CancellationToken token)
        {
            StyleHandler style = new StyleHandler();
            StringBuilder data = new StringBuilder();

            IsConnected = true;

            if (Verbose)
            {
                if (IOActionType == IOAction.Transmit)
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
                if (IOActionType == IOAction.Transmit)
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
