using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;

namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Pipeline for external process standard stream data
    /// </summary>
    internal class ProcessPipe : Pipeline, IConnectable
    {
        /// <summary>
        ///  Initialize object
        /// </summary>
        public ProcessPipe(StreamReader? src, StreamWriter? dest) : base()
        {
            Source = src ?? throw new InvalidOperationException(nameof(src));
            Dest = dest ?? throw new InvalidOperationException(nameof(dest));
        }

        /// <summary>
        ///  Cleanup resources
        /// </summary>
        ~ProcessPipe() => Dispose();

        /// <summary>
        ///  Connect pipeline and activate async communication
        /// </summary>
        protected override async Task ConnectAsync(CancellationToken token)
        {
            StringBuilder data = new();

            int charsRead;
            Connected = true;

            while (Client is not null && Client.Connected)
            {
                // Connection cancellation requested
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                charsRead = await ReadAsync(token);
                data.Append(Buffer.ToArray(), 0, charsRead);

                // Client disconnected
                if (!Client.Connected || (charsRead <= 0))
                {
                    Disconnect();
                    break;
                }

                // Write buffered data to stream
                await WriteAsync(FixLineEndings(data), token);
                data.Clear();
            }

            if (!Program.UsingExe)
            {
                Console.WriteLine();
            }
            Dispose();
        }
    }
}
