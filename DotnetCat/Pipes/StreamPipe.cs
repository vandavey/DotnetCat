using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Handlers;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Handle stream communication operations
    /// </summary>
    class StreamPipe : IConnectable
    {
        private readonly string[] _clearCommands;

        /// Initialize new StreamPipe
        protected StreamPipe()
        {
            _clearCommands = new string[]
            {
                "cls", "clear", "clear-screen"
            };

            this.Client = Program.SockShell.Client;
            this.PlatformType = Program.PlatformType;
            this.IsConnected = false;
        }

        public bool IsConnected { get; protected set; }

        protected TcpClient Client { get; }

        protected Platform PlatformType { get; }

        protected StreamReader Source { get; set; }

        protected StreamWriter Dest { get; set; }

        protected CancellationTokenSource CTS { get; set; }

        protected Task Worker { get; set; }

        /// Activate communication between the pipe streams
        public virtual void Connect()
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

        /// Cancel communication throughout pipe
        public virtual void Disconnect()
        {
            IsConnected = false;
            CTS?.Cancel();
        }

        /// Release any unmanaged resources
        public virtual void Dispose()
        {
            Source?.Dispose();
            Dest?.Dispose();

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
            Memory<char> buffer = new Memory<char>(new char[1024]);

            StringBuilder data = new StringBuilder();
            StringBuilder newLine = new StringBuilder().AppendLine();

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
                data.Append(buffer.ToArray(), 0, charsRead);

                if (!Client.Connected || (charsRead <= 0))
                {
                    Disconnect();
                    break;
                }

                FixLineEndings(data);

                if (IsClearCmd(data.ToString()))
                {
                    await Dest.WriteAsync(newLine, token);
                }
                else
                {
                    await Dest.WriteAsync(data, token);
                }

                await Dest.FlushAsync();
                data.Clear();
            }

            Dispose();
        }

        /// Fix line terminators based on OS platform
        private StringBuilder FixLineEndings(StringBuilder data)
        {
            if (PlatformType == Platform.Windows)
            {
                return data;
            }

            return data.Replace("\r\n", "\n");
        }

        /// Determine if data contains clear command
        private bool IsClearCmd(string data)
        {
            data = data.Replace(Environment.NewLine, "");

            if (_clearCommands.Contains(data.Trim()))
            {
                Console.Clear();
                return true;
            }

            return false;
        }
    }
}
