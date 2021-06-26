using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Contracts;
using DotnetCat.Controllers;
using DotnetCat.Enums;
using DotnetCat.Utils;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Nodes
{
    /// <summary>
    ///  Server node for TCP socket connections
    /// </summary>
    internal class ServerNode : Node, ISockErrorHandled
    {
        private Socket _listener;  // Listener socket

        /// <summary>
        ///  Initialize object
        /// </summary>
        public ServerNode() : base(IPAddress.Any) => _listener = default;

        /// <summary>
        ///  Cleanup resources
        /// </summary>
        ~ServerNode() => Dispose();

        /// <summary>
        ///  Listen for incoming TCP connections
        /// </summary>
        public override void Connect()
        {
            _ = Addr ?? throw new ArgNullException(nameof(Addr));
            HostEndPoint remoteEP = new();

            ValidateArgCombinations();

            // Bind listener socket to local endpoint
            BindListener(new IPEndPoint(Addr, Port));

            try  // Listen for inbound connection
            {
                _listener.Listen(1);
                Style.Info("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                // Start executable process
                if (Program.UsingExe && !StartProcess(Exe))
                {
                    PipeError(Except.ExeProcess, Exe);
                }

                IPEndPoint ep = Client.Client.RemoteEndPoint as IPEndPoint;
                remoteEP = new HostEndPoint(ep);

                Style.Info($"Connected to {remoteEP}");

                base.Connect();
                WaitForExit();

                // Connection closed status
                Style.Info($"Connection to {remoteEP} closed");
            }
            catch (SocketException ex)  // Error (likely refused)
            {
                PipeError(Except.ConnectionRefused,
                          remoteEP,
                          ex,
                          Level.Warn);
            }
            catch (IOException ex)      // Connection lost
            {
                PipeError(Except.ConnectionLost, remoteEP, ex);
            }

            Dispose();
        }

        /// <summary>
        ///  Dispose of unmanaged resources and handle error
        /// </summary>
        public override void PipeError(Except type,
                                       HostEndPoint target,
                                       Exception ex = default,
                                       Level level = default) {
            Dispose();
            Error.Handle(type, target.ToString(), ex, level);
        }

        /// <summary>
        ///  Dispose of unmanaged resources and handle error
        /// </summary>
        public override void PipeError(Except type,
                                       string arg,
                                       Exception ex = default,
                                       Level level = default) {
            Dispose();
            Error.Handle(type, arg, ex, level);
        }

        /// <summary>
        ///  Release any unmanaged resources
        /// </summary>
        public override void Dispose()
        {
            _listener?.Dispose();
            base.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Bind the listener socket to an endpoint
        /// </summary>
        private void BindListener(IPEndPoint ep)
        {
            _ = ep ?? throw new ArgNullException(nameof(ep));

            _listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

            try  // Bind socket to endpoint
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
