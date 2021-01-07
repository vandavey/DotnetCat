using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Server node for TCP socket connections
    /// </summary>
    class ServerNode : Node, IErrorHandled
    {
        private Socket _listener;

        /// Initialize new object
        public ServerNode() : base(address: IPAddress.Any)
        {
            _listener = null;
        }

        /// Listen for incoming TCP connections
        public override void Connect()
        {
            // Bind listener socket to local endpoint
            BindListener(new IPEndPoint(Addr, Port));
            IPEndPoint remoteEP = null;

            try // Listen for connection
            {
                _listener.Listen(1);
                Style.Status("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                // Start executable process
                if (Program.UsingExe)
                {
                    Exe ??= Cmd.GetDefaultExe(OS);
                    bool hasStarted = Start(Exe);

                    if (!hasStarted)
                    {
                        PipeError(Except.ExecProcess, Exe);
                    }
                }

                remoteEP = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connected to {remoteEP}");

                base.Connect();
                WaitForExit();

                // Connection closed status
                Style.Status($"Connection to {remoteEP.Address} closed");
            }
            catch (SocketException ex) // Connection refused
            {
                PipeError(Except.ConnectionRefused, $"{remoteEP}", ex);
            }
            catch (IOException ex) // Connection lost
            {
                PipeError(Except.ConnectionLost, $"{remoteEP}", ex);
            }
            Dispose();
        }

        /// Dispose of unmanaged resources and handle error
        public override void PipeError(Except type, string arg,
                                                    Exception ex = null) {
            Dispose();
            Error.Handle(type, arg, ex);
        }

        /// Release any unmanaged resources
        public override void Dispose()
        {
            _listener?.Dispose();
            base.Dispose();
        }

        /// Bind the listener socket to an endpoint
        private void BindListener(IPEndPoint ep)
        {
            _ = ep ?? throw new ArgNullException(nameof(ep));

            _listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

            try // Bind socket to endpoint
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
