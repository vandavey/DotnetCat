using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Handlers;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Specify if node will send/receive files
    /// </summary>
    enum NodeAction { Send, Receive, None }

    /// <summary>
    /// Handle file data communication
    /// </summary>
    class FilePipe : StreamPipe, ICloseable
    {
        private readonly ErrorHandler _error;

        /// Initialize new FilePipe
        public FilePipe(StreamReader src, string path) : base()
        {
            _error = new ErrorHandler();

            this.Source = src ?? throw new ArgumentNullException("src");
            this.NodeAction = NodeAction.Receive;

            this.Dest = new StreamWriter(CreateFile(path, _error));
            this.Client = Program.SockShell.Client;
        }

        /// Initialize new FilePipe
        public FilePipe(string path, StreamWriter dest) : base()
        {
            _error = new ErrorHandler();

            this.Dest = dest ?? throw new ArgumentNullException("dest");
            this.NodeAction = NodeAction.Send;

            this.Source = new StreamReader(OpenFile(path, _error));
            this.Client = Program.SockShell.Client;
        }

        public NodeAction NodeAction { get; }

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
                error.Handle("emptypath", "-r/--recv");
            }

            DirectoryInfo info = Directory.GetParent(path);

            if (!Directory.Exists(info.FullName))
            {
                error.Handle("dirpath", info.FullName);
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
                error.Handle("emptypath", "-s/--send");
            }

            FileInfo info = new FileInfo(path);

            if (!File.Exists(info.FullName))
            {
                error.Handle("filepath", info.FullName);
            }

            return new FileStream(
                path, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 4096, useAsync: true
            );
        }

        /// Activate async communication
        private async Task ConnectAsync(CancellationToken token)
        {
            StringBuilder data = new StringBuilder();
            IsConnected = true;

            data.Append(await Source.ReadToEndAsync());
            await Dest.WriteAsync(data, token);

            await Dest.FlushAsync();
            Disconnect();

            Close();
        }
    }
}
