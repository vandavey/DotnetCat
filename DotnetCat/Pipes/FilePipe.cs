using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Handle file data streams
    /// </summary>
    class FilePipe : StreamPipe, ICloseable
    {
        /// Initialize new FilePipe
        public FilePipe(StreamReader source, string path) : base()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            else if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("filePath");
            }

            this.DestPath = path;
            this.Source = source;
            this.TransferType = "recv";

            this.Client = Program.SockShell.Client;
            this.Destination = new StreamWriter(CreateFile(path));
        }

        /// Initialize new FilePipe
        public FilePipe(string path, StreamWriter dest) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("filePath");
            }
            else if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }

            this.SourcePath = path;
            this.Destination = dest;
            this.TransferType = "send";

            FileStream stream = OpenFile(path);
            this.Source = new StreamReader(stream);
        }

        public string SourcePath { get; set; }

        public string DestPath { get; set; }

        public string TransferType { get; set; }

        /// Activate communication between the pipe streams
        public override void Connect()
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }
            else if (Destination == null)
            {
                throw new ArgumentNullException("Destination");
            }

            CTS = new CancellationTokenSource();
            Worker = ConnectAsync(CTS.Token);
        }

        /// Create and open new file for writing
        private static FileStream CreateFile(string filePath)
        {
            return new FileStream(
                filePath, FileMode.Create, FileAccess.Write,
                FileShare.Write, bufferSize: 1024, useAsync: true
            );
        }

        /// Open specified FileStream for reading/writing
        private static FileStream OpenFile(string filePath)
        {
            return new FileStream(
                filePath, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 4096, useAsync: true
            );
        }

        /// Activate async communication
        private async Task ConnectAsync(CancellationToken token)
        {
            IsConnected = true;

            StringBuilder data = new StringBuilder();
            data.Append(await Source.ReadToEndAsync());

            await Destination.WriteAsync(data, token);
            await Destination.FlushAsync();

            Disconnect();
            Close();
        }
    }
}
