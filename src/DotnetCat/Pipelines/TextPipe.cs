using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.IO;

namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Pipeline for user defined string data
    /// </summary>
    internal class TextPipe : Pipeline, IConnectable
    {
        private string _payload;             // String payload

        private MemoryStream _memoryStream;  // Memory buffer

        /// <summary>
        ///  Initialize object
        /// </summary>
        public TextPipe(string? data, StreamWriter? dest) : base()
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            _memoryStream = new MemoryStream();

            Payload = _payload = data;
            StatusMsg = "Payload successfully transmitted";

            Dest = dest ?? throw new ArgumentNullException(nameof(dest));
            Source = new StreamReader(_memoryStream);
        }

        /// <summary>
        ///  Cleanup resources
        /// </summary>
        ~TextPipe() => Dispose();

        /// String network payload
        protected string Payload
        {
            get => _payload ?? string.Empty;
            set
            {
                _payload = value ?? throw new ArgumentException(null, nameof(value));
                _memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            }
        }

        /// Completion status message
        protected string StatusMsg { get; set; }

        /// <summary>
        ///  Release any unmanaged resources
        /// </summary>
        public override void Dispose()
        {
            _memoryStream?.Dispose();
            base.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Activate async network communication
        /// </summary>
        protected override async Task ConnectAsync(CancellationToken token)
        {
            Connected = true;

            StringBuilder data = new(await ReadToEndAsync());
            await WriteAsync(data, token);

            if (Program.Verbose)
            {
                Style.Output(StatusMsg);
            }

            Disconnect();
            Dispose();
        }
    }
}
