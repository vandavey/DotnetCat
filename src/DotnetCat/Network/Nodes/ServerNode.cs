using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes
{
    /// <summary>
    ///  Server socket node with an underlying TCP socket client and listener socket.
    /// </summary>
    internal class ServerNode : Node
    {
        private Socket? _listener;  // Listener socket

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public ServerNode() : base(IPAddress.Any) => _listener = default;

        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public ServerNode(CmdLineArgs args) : base(args) => _listener = default;

        /// <summary>
        ///  Release the unmanaged object resources.
        /// </summary>
        ~ServerNode() => Dispose();

        /// <summary>
        ///  Listen for an inbound TCP connection on the underlying listener socket.
        /// </summary>
        public override void Connect()
        {
            _ = Address ?? throw new ArgumentNullException(nameof(Address));
            HostEndPoint remoteEP = new();

            ValidateArgsCombinations();
            BindListener(new IPEndPoint(Address, Port));

            try  // Listen for an inbound connection
            {
                _listener?.Listen(1);
                Style.Info("Listening for incoming connections...");

                if (_listener is not null)
                {
                    Client.Client = _listener.Accept();
                }
                NetStream = Client.GetStream();

                // Start the executable process
                if (Args.UsingExe && !StartProcess(ExePath))
                {
                    PipeError(Except.ExeProcess, ExePath);
                }

                IPEndPoint? ep = Client.Client.RemoteEndPoint as IPEndPoint;
                remoteEP = new HostEndPoint(ep);

                Style.Info($"Connected to {remoteEP}");

                base.Connect();
                WaitForExit();

                Console.WriteLine();
                Style.Info($"Connection to {remoteEP} closed");
            }
            catch (SocketException ex)  // Error (likely refused)
            {
                PipeError(Except.ConnectionRefused, remoteEP, ex);
            }
            catch (IOException ex)      // Connection lost
            {
                PipeError(Except.ConnectionLost, remoteEP, ex);
            }

            Dispose();
        }

        /// <summary>
        ///  Dispose of all unmanaged resources and handle the given error.
        /// </summary>
        public override void PipeError(Except type,
                                       HostEndPoint target,
                                       Exception? ex = default,
                                       Level level = default) {
            Dispose();
            Error.Handle(type, target.ToString(), ex, level);
        }

        /// <summary>
        ///  Dispose of all unmanaged resources and handle the given error.
        /// </summary>
        public override void PipeError(Except type,
                                       string? arg,
                                       Exception? ex = default,
                                       Level level = default) {
            Dispose();
            Error.Handle(type, arg, ex, level);
        }

        /// <summary>
        ///  Release all the underlying unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _listener?.Close();
            base.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Bind the underlying listener socket to the given IPv4 endpoint.
        /// </summary>
        private void BindListener(IPEndPoint ep)
        {
            _ = ep ?? throw new ArgumentNullException(nameof(ep));

            _listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

            try  // Bind the listener socket
            {
                _listener.Bind(ep);
            }
            catch (SocketException ex)
            {
                PipeError(Except.SocketBind, ep.ToString(), ex);
            }
        }
    }
}
