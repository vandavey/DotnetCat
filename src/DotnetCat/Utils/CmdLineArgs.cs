using System.Net;
using DotnetCat.IO.Pipelines;

namespace DotnetCat.Utils
{
    /// <summary>
    ///  DotnetCat command-line arguments.
    /// </summary>
    internal class CmdLineArgs
    {
        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public CmdLineArgs()
        {
            Debug = false;
            Listen = false;
            Verbose = false;

            PipeVariant = PipeType.Stream;
            TransOpt = TransferOpt.None;
            Port = 44444;

            ExePath = default;
            FilePath = default;
            Payload = default;

            Address = IPAddress.Any;
        }

        /// Enable verbose exceptions
        public bool Debug { get; set; }

        /// Run server and listen for inbound connection
        public bool Listen { get; set; }

        /// Using executable pipeline
        public bool UsingExe => !ExePath.IsNullOrEmpty();

        /// Enable verbose console output
        public bool Verbose { get; set; }

        /// Pipeline variant
        public PipeType PipeVariant { get; set; }

        /// File transfer option
        public TransferOpt TransOpt { get; set; }

        /// Connection port number
        public int Port { get; set; }

        /// Executable file path
        public string? ExePath { get; set; }

        /// Transfer file path
        public string? FilePath { get; set; }

        /// Hostname of the connection IPv4 address
        public string? HostName { get; set; }

        /// User-defined string payload
        public string? Payload { get; set; }

        /// IPv4 address to use for connection
        public IPAddress Address { get; set; }
    }
}
