using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Controllers;
using DotnetCat.Enums;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Base class for all pipelines in the Pipelines namespace
    /// </summary>
    internal class Pipeline : IConnectable
    {
        /// <summary>
        ///  Initialize object
        /// </summary>
        protected Pipeline()
        {
            Connected = false;
            NewLine = new StringBuilder(Environment.NewLine);
        }

        /// <summary>
        ///  Cleanup resources
        /// </summary>
        ~Pipeline() => Dispose();

        /// Pipeline is active
        public bool Connected { get; protected set; }

        /// Operating system
        protected static Platform OS => Program.OS;

        /// Operating system
        protected static TcpClient Client => Program.SockNode.Client;

        /// Platform based EOL escape sequence
        protected StringBuilder NewLine { get; }

        /// Pipeline cancellation token
        protected CancellationTokenSource CTS { get; set; }

        /// Pipeline data source
        protected StreamReader Source { get; set; }

        /// Pipeline data destination
        protected StreamWriter Dest { get; set; }

        /// Pipeline data transfer task
        protected Task Worker { get; set; }

        /// <summary>
        ///  Activate communication between the pipe streams
        /// </summary>
        public virtual void Connect()
        {
            _ = Source ?? throw new ArgNullException(nameof(Source));
            _ = Dest ?? throw new ArgNullException(nameof(Dest));

            CTS = new CancellationTokenSource();
            Worker = ConnectAsync(CTS.Token);
        }

        /// <summary>
        ///  Cancel communication throughout pipe
        /// </summary>
        public virtual void Disconnect()
        {
            Connected = false;
            CTS?.Cancel();
        }

        /// <summary>
        ///  Release any unmanaged resources
        /// <summary>
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

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Connect pipeline and activate async communication
        /// </summary>
        protected virtual async Task ConnectAsync(CancellationToken token)
        {
            StringBuilder data = new();
            Memory<char> buffer = new(new char[1024]);

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
                data = FixLineEndings(data);  // Normalize EOL sequences

                // Clear console buffer if requested
                if (Command.IsClearCmd(data.ToString()))
                {
                    await Dest.WriteAsync(NewLine, token);
                }
                else
                {
                    await Dest.WriteAsync(data, token);
                }
                data.Clear();
            }

            Dispose();
        }

        /// <summary>
        ///  Fix line terminators based on OS platform
        /// </summary>
        protected static StringBuilder FixLineEndings(StringBuilder data)
        {
            return (OS is Platform.Win) ? data : data.Replace("\r\n", "\n");
        }
    }
}
