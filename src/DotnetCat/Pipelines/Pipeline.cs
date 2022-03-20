using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Shell.Commands;
using DotnetCat.Utils;

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
        protected static TcpClient? Client => Program.SockNode?.Client;

        /// TCP client is connected
        protected static bool ClientConnected => Client?.Connected ?? false;

        /// Platform based EOL escape sequence
        protected StringBuilder NewLine { get; }

        /// Pipeline cancellation token
        protected CancellationTokenSource? CTS { get; set; }

        /// Character memory buffer
        protected Memory<char> Buffer { get; set; }

        /// Pipeline data source
        protected StreamReader? Source { get; set; }

        /// Pipeline data destination
        protected StreamWriter? Dest { get; set; }

        /// Pipeline data transfer task
        protected Task? Worker { get; set; }

        /// <summary>
        ///  Activate communication between the pipe streams
        /// </summary>
        public virtual void Connect()
        {
            _ = Source ?? throw new InvalidOperationException(nameof(Source));
            _ = Dest ?? throw new InvalidOperationException(nameof(Dest));

            CTS = new CancellationTokenSource();
            Buffer = new Memory<char>(new char[1024]);

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

            int charsRead;
            Connected = true;

            if (Client is not null)
            {
                while (Client.Connected)
                {
                    // Connection cancellation requested
                    if (token.IsCancellationRequested)
                    {
                        Disconnect();
                        break;
                    }

                    if (Source is not null)
                    {
                        charsRead = await Source.ReadAsync(Buffer, token);
                        data.Append(Buffer.ToArray(), 0, charsRead);

                        // Client disconnected
                        if (!Client.Connected || (charsRead <= 0))
                        {
                            Disconnect();
                            break;
                        }
                    }
                    data = FixLineEndings(data);  // Normalize EOL sequences

                    // Clear console buffer if requested
                    if (Dest is not null)
                    {
                        if (Command.IsClearCmd(data.ToString()))
                        {
                            await Dest.WriteAsync(NewLine, token);
                        }
                        else
                        {
                            await Dest.WriteAsync(data, token);
                        }
                    }
                    data.Clear();
                }
                Dispose();
            }
        }

        /// <summary>
        ///  Fix line terminators based on OS platform
        /// </summary>
        protected static StringBuilder FixLineEndings(StringBuilder data)
        {
            return (OS is Platform.Win) ? data : data.Replace("\r\n", "\n");
        }

        /// <summary>
        ///  Asynchronously read stream data into the underlying memory buffer
        /// </summary>
        protected virtual async ValueTask<int> ReadAsync(CancellationToken token)
        {
            int bytesRead = -1;

            if (Source is not null && ClientConnected)
            {
                bytesRead = await Source.ReadAsync(Buffer, token);
            }
            return bytesRead;
        }

        /// <summary>
        ///  Asynchronously read all the data from the source stream
        /// </summary>
        protected virtual async ValueTask<string> ReadToEndAsync()
        {
            string buffer = string.Empty;

            if (Source is not null && ClientConnected)
            {
                buffer = await Source.ReadToEndAsync();
            }
            return buffer;
        }

        /// <summary>
        ///  Asynchronously write the given data to the destination stream
        /// </summary>
        protected virtual async Task WriteAsync(StringBuilder data,
                                                CancellationToken token) {

            if (Dest is not null && ClientConnected)
            {
                await Dest.WriteAsync(data, token);
            }
        }
    }
}
