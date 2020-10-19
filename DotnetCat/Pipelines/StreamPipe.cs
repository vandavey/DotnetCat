using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Base class for all pipelines in DotnetCat.Pipelines
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
            this.Platform = Program.Platform;
            this.Connected = false;
        }

        public bool Connected { get; protected set; }

        protected TcpClient Client { get; }

        protected PlatformType Platform { get; }

        protected StreamReader Source { get; set; }

        protected StreamWriter Dest { get; set; }

        protected CancellationTokenSource CTS { get; set; }

        protected Task Worker { get; set; }

        /// Activate communication between the pipe streams
        public virtual void Connect()
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

        /// Cancel communication throughout pipe
        public virtual void Disconnect()
        {
            Connected = false;
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
            Connected = true;

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
            if (Platform == PlatformType.Windows)
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
