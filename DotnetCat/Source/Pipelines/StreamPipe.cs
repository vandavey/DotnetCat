using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Base class for all pipelines in DotnetCat.Pipelines
    /// </summary>
    class StreamPipe : IConnectable
    {
        /// Initialize object
        protected StreamPipe()
        {
            Connected = false;
        }

        /// Cleanup resources
        ~StreamPipe() => Dispose();

        public bool Connected { get; protected set; }

        protected static Platform OS => Program.OS;

        protected static TcpClient Client => Program.SockNode.Client;

        protected CancellationTokenSource CTS { get; set; }

        protected StreamReader Source { get; set; }

        protected StreamWriter Dest { get; set; }

        protected Task Worker { get; set; }

        /// Activate communication between the pipe streams
        public virtual void Connect()
        {
            _ = Source ?? throw new ArgNullException(nameof(Source));
            _ = Dest ?? throw new ArgNullException(nameof(Dest));

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

            try  // Try to dispose of task
            {
                Worker?.Dispose();
            }
            catch (InvalidOperationException)
            {
            }

            // Prevent unnecessary finalization
            GC.SuppressFinalize(this);
        }

        /// Connect pipelines and activate async communication
        protected virtual async Task ConnectAsync(CancellationToken token)
        {
            Memory<char> buffer = new(new char[1024]);

            StringBuilder data = new();
            StringBuilder newLine = new StringBuilder().AppendLine();

            int charsRead;
            Connected = true;

            while (Client.Connected)
            {
                // Connection cancellation requested
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                charsRead = await Source.ReadAsync(buffer, token);
                data.Append(buffer.ToArray(), 0, charsRead);

                // Client disconnected
                if (!Client.Connected || (charsRead <= 0))
                {
                    Disconnect();
                    break;
                }
                FixLineEndings(data);  // Normalize EOL sequences

                // Clear console buffer if requested
                if (CommandHandler.IsClearCmd(data.ToString()))
                {
                    await Dest.WriteAsync(newLine, token);
                }
                else
                {
                    await Dest.WriteAsync(data, token);
                }
                data.Clear();
            }


            if (!Program.UsingExe)
            {
                Console.WriteLine();
            }
            Dispose();
        }

        /// Fix line terminators based on OS platform
        private static StringBuilder FixLineEndings(StringBuilder data)
        {
            return (OS is Platform.Win) ? data : data.Replace("\r\n", "\n");
        }
    }
}
