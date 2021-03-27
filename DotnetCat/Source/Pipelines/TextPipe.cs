using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Handlers;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline for user defined string data
    /// </summary>
    class TextPipe : StreamPipe, IConnectable
    {
        private readonly MemoryStream _memStream;

        /// <summary>
        /// Initialize object
        /// </summary>
        public TextPipe(string data, StreamWriter dest) : base()
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgNullException(nameof(data));
            }
            _memStream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            Dest = dest ?? throw new ArgNullException(nameof(dest));
            Source = new StreamReader(_memStream);
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        ~TextPipe() => Dispose();

        /// <summary>
        /// Release any unmanaged resources
        /// </summary>
        public override void Dispose()
        {
            _memStream?.Dispose();
            base.Dispose();

            // Prevent unnecessary finalization
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Activate async network communication
        /// </summary>
        protected override async Task ConnectAsync(CancellationToken token)
        {
            Connected = true;
            StringBuilder data = new();

            data.Append(await Source.ReadToEndAsync());
            await Dest.WriteAsync(data, token);

            if (Program.Verbose)
            {
                StyleHandler.Output("Payload successfully transmitted");
            }

            Disconnect();
            Dispose();
        }
    }
}
