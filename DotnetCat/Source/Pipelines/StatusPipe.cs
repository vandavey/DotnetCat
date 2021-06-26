using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Controllers;
using DotnetCat.Nodes;

namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Pipeline class for connection testing
    /// </summary>
    internal class StatusPipe : TextPipe, IConnectable
    {
        /// <summary>
        ///  Initialize object
        /// </summary>
        public StatusPipe(StreamWriter dest) : base(string.Empty, dest)
        {
            Node node = Program.SockNode;
            string target = $"{node.DestName}:{node.Port}";

            StatusMsg = $"Connection accepted by {target}";
        }

        /// <summary>
        ///  Activate async network communication
        /// </summary>
        protected override async Task ConnectAsync(CancellationToken token)
        {
            Connected = true;

            StringBuilder data = new(await Source.ReadToEndAsync());
            await Dest.WriteAsync(data, token);

            Style.Output(StatusMsg);

            Disconnect();
            Dispose();
        }
    }
}
