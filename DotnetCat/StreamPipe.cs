using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCat
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
        public StreamPipe(TcpClient client, Stream source, Stream dest)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            else if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            else if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }

            _client = client;

            this.SourceStream = source;
            this.DestStream = dest;
            this.IsConnected = false;
            this.IsTransfer = false;
        }

        public Stream SourceStream { get; }

        public Stream DestStream { get; }

        public bool IsTransfer { get; set; }

        public bool IsConnected { get; private set; }

        /// Activate communication between the pipe streams
        public void Connect(CancellationTokenSource cts = null)
        {
            _cts = cts ?? new CancellationTokenSource();
            _worker = ConnectAsync(_cts.Token);
        }

        /// Cancel communication between streams
        public void Disconnect()
        {
            _cts?.Cancel();
            IsConnected = false;
        }

        /// Release any unmanaged resources
        public void Close()
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
        private async Task ConnectAsync(CancellationToken token)
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

            if (IsTransfer)
            {
                await TransferFileAsync(token);
                return;
            }

            int bytesRead;
            byte[] buff = new byte[1024];

            // Primary data communication loop
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

        /// Transfer a file over socket stream
        private async Task TransferFileAsync(CancellationToken token)
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
    }
}
