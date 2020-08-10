using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Handle stream communication operations
    /// </summary>
    class StreamPipe : ICloseable
    {
        /// Initialize new StreamPipe
        protected StreamPipe()
        {
            this.IsConnected = false;
            this.Client = Program.SockShell.Client;
        }

        public bool IsConnected { get; protected set; }

        protected Task Worker { get; set; }

        protected StreamReader Source { get; set; }

        protected StreamWriter Destination { get; set; }

        protected TcpClient Client { get; set; }

        protected CancellationTokenSource CTS { get; set; }

        /// Activate communication between the pipe streams
        public virtual void Connect()
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }
            else if (Destination == null)
            {
                throw new ArgumentNullException("Dest");
            }

            CTS = new CancellationTokenSource();
            Worker = ConnectAsync(CTS.Token);
        }

        /// Cancel communication throughout pipe
        public virtual void Disconnect()
        {
            IsConnected = false;
            CTS?.Cancel();
        }

        /// Release any unmanaged resources
        public virtual void Close()
        {
            Source?.Dispose();
            Destination?.Dispose();

            CTS?.Dispose();
            Client?.Dispose();

            try
            {
                Worker?.Dispose();
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        /// Connect streams and activate async communication
        private async Task ConnectAsync(CancellationToken token)
        {
            // TODO: fix issue with linux line endings
            StringBuilder streamData = new StringBuilder();
            Memory<char> buffer = new Memory<char>(new char[1024]);

            int charsRead;
            IsConnected = true;

            // Primary data communication loop
            while (Client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                charsRead = await Source.ReadAsync(buffer, token);
                streamData.Append(buffer.ToArray(), 0, charsRead);

                if (!Client.Connected || (charsRead <= 0))
                {
                    Disconnect();
                    break;
                }

                await Destination.WriteAsync(streamData, token);
                await Destination.FlushAsync();

                streamData.Clear();
            }

            Close();
        }
    }
}
