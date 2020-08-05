using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Handle binary communication between two streams
    /// </summary>
    class StreamPipe : ICloseable
    {
        private readonly TcpClient _client;

        private Task _worker;

        private CancellationTokenSource _cts;

        /// Initialize new StreamPipe
        public StreamPipe(TcpClient client, Stream src, Stream dest)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            else if (src == null)
            {
                throw new ArgumentNullException("source");
            }
            else if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }

            _client = client;

            this.SourceStream = src;
            this.DestStream = dest;
            this.IsConnected = false;
            this.IsFileTransfer = false;
        }

        public Stream SourceStream { get; }

        public Stream DestStream { get; }

        public bool IsFileTransfer { get; set; }

        public bool IsConnected { get; private set; }

        /// Activate communication between the pipe streams
        public virtual void Connect(CancellationTokenSource cts = null)
        {
            _cts = cts ?? new CancellationTokenSource();
            _worker = ConnectAsync(_cts.Token);
        }

        /// Cancel communication between streams
        public virtual void Disconnect()
        {
            _cts?.Cancel();
            IsConnected = false;
        }

        /// Release any unmanaged resources
        public virtual void Close()
        {
            SourceStream?.Dispose();
            DestStream?.Dispose();

            _cts?.Dispose();
            _client?.Dispose();

            try
            {
                _worker?.Dispose();
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        /// Connect streams and activate async communication
        protected async Task ConnectAsync(CancellationToken token)
        {
            // TODO: fix issue with linux line-ending issues
            if (SourceStream == null)
            {
                throw new ArgumentNullException("SourceStream");
            }
            else if (DestStream == null)
            {
                throw new ArgumentNullException("DestStream");
            }

            IsConnected = true;

            if (IsFileTransfer)
            {
                await TransferAsync(token);
            }

            await CommunicateAsync(token);
        }

        /// Transfer file data over socket stream
        protected async Task TransferAsync(CancellationToken token)
        {
            StringBuilder data = new StringBuilder();

            using (StreamReader reader = new StreamReader(SourceStream))
            using (StreamWriter writer = new StreamWriter(DestStream))
            {
                data.Append(await reader.ReadToEndAsync());

                await writer.WriteAsync(data, token);
                await writer.FlushAsync();
            }

            Disconnect();
        }

        /// Transfer shell process data over socket stream
        protected async Task CommunicateAsync(CancellationToken token)
        {
            int bytesRead;
            byte[] buff = new byte[1024];

            while (_client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                bytesRead = await SourceStream.ReadAsync(
                    buff, 0, buff.Length, token
                );

                if (!_client.Connected || (bytesRead <= 0))
                {
                    Disconnect();
                    break;
                }

                await DestStream.WriteAsync(buff, 0, bytesRead, token);
                await DestStream.FlushAsync(token);
            }
        }
    }
}
