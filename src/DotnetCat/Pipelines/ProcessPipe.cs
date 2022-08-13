using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Utils;

namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Stream pipeline used to transfer executable process data.
    /// </summary>
    internal class ProcessPipe : Pipeline, IConnectable
    {
        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public ProcessPipe(CmdLineArgs args, StreamReader? src, StreamWriter? dest)
            : base(args) {

            Source = src ?? throw new InvalidOperationException(nameof(src));
            Dest = dest ?? throw new InvalidOperationException(nameof(dest));
        }

        /// <summary>
        ///  Release the unmanaged object resources.
        /// </summary>
        ~ProcessPipe() => Dispose();

        /// <summary>
        ///  Asynchronously transfer the executable process data
        ///  between the underlying streams.
        /// </summary>
        protected override async Task ConnectAsync(CancellationToken token)
        {
            StringBuilder data = new();

            int charsRead;
            Connected = true;

            while (Client is not null && Client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                charsRead = await ReadAsync(token);
                data.Append(Buffer.ToArray(), 0, charsRead);

                // Socket client was disconnected
                if (!Client.Connected || charsRead <= 0)
                {
                    Disconnect();
                    break;
                }

                await WriteAsync(FixLineEndings(data), token);
                data.Clear();
            }

            if (!Args.UsingExe)
            {
                Console.WriteLine();
            }
            Dispose();
        }
    }
}
