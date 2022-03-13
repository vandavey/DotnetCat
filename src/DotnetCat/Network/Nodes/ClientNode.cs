using System;
using System.IO;
using System.Net.Sockets;
using DotnetCat.Contracts;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Pipelines;

namespace DotnetCat.Network.Nodes
{
    /// <summary>
    ///  Client node for TCP socket connections
    /// </summary>
    internal class ClientNode : Node, ISockErrorHandled
    {
        private HostEndPoint _targetEP;  // Remote target

        /// <summary>
        ///  Initialize object
        /// </summary>
        public ClientNode() : base() => _targetEP = default;

        /// <summary>
        ///  Cleanup resources
        /// </summary>
        ~ClientNode() => Dispose();

        /// <summary>
        ///  Connect to the specified IPv4 address and port number
        /// </summary>
        public override void Connect()
        {
            _ = Addr ?? throw new ArgumentNullException(nameof(Addr));
            _targetEP = new HostEndPoint(DestName, Port);

            ValidateArgCombinations();

            try  // Connect with timeout
            {
                if (!Client.ConnectAsync(Addr, Port).Wait(3500))
                {
                    throw new SocketException(10060);  // Socket timeout
                }
                NetStream = Client.GetStream();

                // Start executable process
                if (Program.UsingExe && !StartProcess(Exe))
                {
                    PipeError(Except.ExeProcess, Exe);
                }

                if (Program.PipeVariant is not PipeType.Status)
                {
                    Style.Info($"Connected to {_targetEP}");
                }

                base.Connect();
                WaitForExit();

                // Connection closed status
                Style.Info($"Connection to {_targetEP} closed");
            }
            catch (AggregateException ex)  // Connection refused
            {
                PipeError(Except.ConnectionRefused, _targetEP, ex, Level.Warn);
            }
            catch (SocketException ex)     // Error (likely timeout)
            {
                PipeError(Except.ConnectionTimeout, _targetEP, ex, Level.Warn);
            }
            catch (IOException ex)         // Connection lost
            {
                PipeError(Except.ConnectionLost, Addr.ToString(), ex);
            }

            Dispose();
        }
    }
}
