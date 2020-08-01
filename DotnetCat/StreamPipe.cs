using System;
using System.IO;
using System.Net.Sockets;
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

        private int _bytesRead;
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
            _bytesRead = -1;

            this.SourceStream = source;
            this.DestStream = dest;
            this.IsConnected = false;
        }

        public Stream SourceStream { get; }

        public Stream DestStream { get; }

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
            if (SourceStream == null)
            {
                throw new ArgumentNullException("SourceStream");
            }
            else if (DestStream == null)
            {
                throw new ArgumentNullException("DestStream");
            }

            IsConnected = true;
            byte[] buff = new byte[1024];

            // Primary stream communication loop
            while (_client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                _bytesRead = await SourceStream.ReadAsync(
                    buff, 0, buff.Length, token
                );

                if (!_client.Connected || (_bytesRead <= 0))
                {
                    Disconnect();
                    break;
                }

                await DestStream.WriteAsync(buff, 0, _bytesRead, token);
                await DestStream.FlushAsync(token);
            }
        }
    }
}
