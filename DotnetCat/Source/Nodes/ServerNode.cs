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
    class ServerNode : SocketNode, IConnectable
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
            IPEndPoint remoteEP;

            try
            {
                _listener.Listen(1);
                Style.Status("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                // Start the executable process
                if (Program.UsingExe)
                {
                    Exe ??= Cmd.GetDefaultExe(OS);
                    bool hasStarted = Start(Exe);

                    if (!hasStarted)
                    {
                        Error.Handle(Except.ExecProcess, Exe);
                    }
                }

                remoteEP = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connected to {remoteEP}");

                base.Connect();
                WaitForExit();
            }
            catch (SocketException ex) // Connection refused
            {
                Dispose();
                Error.Handle(Except.ConnectionRefused, $"{Addr}:{Port}", ex);
            }
            catch (IOException ex) // Connection lost
            {
                Dispose();
                Error.Handle(Except.ConnectionLost, $"{Addr}", ex);
            }
            Dispose();
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
            try
            {
                _listener.Bind(ep);
                return;
            }
            catch (SocketException ex)
            {
                Dispose();
                Error.Handle(Except.SocketBind, ep.ToString(), ex);
            }
        }
    }
}
