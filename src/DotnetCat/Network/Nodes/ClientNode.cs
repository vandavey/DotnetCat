using System;
using System.IO;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes
{
    /// <summary>
    ///  Client socket node with an underlying TCP socket client.
    /// </summary>
    internal class ClientNode : Node
    {
        private HostEndPoint? _targetEP;  // Remote target

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public ClientNode() : base() => _targetEP = default;

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public ClientNode(CmdLineArgs args) : base(args) => _targetEP = default;

        /// <summary>
        ///  Release the unmanaged object resources.
        /// </summary>
        ~ClientNode() => Dispose();

        /// <summary>
        ///  Establish a socket connection to the underlying IPv4 endpoint.
        /// </summary>
        public override void Connect()
        {
            _ = Address ?? throw new ArgumentNullException(nameof(Address));
            _targetEP = new HostEndPoint(HostName, Port);

            ValidateArgsCombinations();

            try  // Connect with timeout
            {
                if (!Client.ConnectAsync(Address, Port).Wait(3500))
                {
                    throw Net.GetException(SocketError.TimedOut);
                }
                NetStream = Client.GetStream();

                // Start the executable process
                if (Args.UsingExe && !StartProcess(ExePath))
                {
                    PipeError(Except.ExeProcess, ExePath);
                }

                if (Args.PipeVariant is not PipeType.Status)
                {
                    Style.Info($"Connected to {_targetEP}");
                }

                base.Connect();
                WaitForExit();

                Style.Info($"Connection to {_targetEP} closed");
            }
            catch (AggregateException ex)  // Connection refused
            {
                PipeError(Except.ConnectionRefused, _targetEP, ex, Level.Error);
            }
            catch (SocketException ex)     // Error (likely timeout)
            {
                PipeError(Except.ConnectionTimeout, _targetEP, ex, Level.Error);
            }
            catch (IOException ex)         // Connection lost
            {
                PipeError(Except.ConnectionLost, Address.ToString(), ex);
            }

            Dispose();
        }
    }
}
