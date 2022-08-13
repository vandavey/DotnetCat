using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.IO;
using DotnetCat.Network.Nodes;
using DotnetCat.Utils;

namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Stream pipeline used to perform network connection testing.
    /// </summary>
    internal class StatusPipe : TextPipe, IConnectable
    {
        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public StatusPipe(CmdLineArgs args, StreamWriter? dest) : base(args, dest)
        {
            if (Program.SockNode is null)
            {
                string msg = $"{nameof(Program.SockNode)} cannot be null";
                throw new InvalidOperationException(msg);
            }

            Node node = Program.SockNode;
            string target = $"{node.HostName}:{node.Port}";

            StatusMsg = $"Connection accepted by {target}";
        }

        /// <summary>
        ///  Asynchronously transfer an empty string between the underlying streams.
        /// </summary>
        protected override async Task ConnectAsync(CancellationToken token)
        {
            Connected = true;

            StringBuilder data = new(await ReadToEndAsync());
            await WriteAsync(data, token);

            Style.Output(StatusMsg);

            Disconnect();
            Dispose();
        }
    }
}
