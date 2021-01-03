using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Contracts;
using DotnetCat.Enums;

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
            IPEndPoint remoteEP;
            BindListener(new IPEndPoint(Addr, Port));

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
                        Error.Handle(Except.ShellProcess, Exe);
                    }
                }

                remoteEP = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connected to {remoteEP}");

                base.Connect();
                WaitForExit();
            }
            catch (SocketException) // Connection refused
            {
                string endPoint = $"{Addr}:{Port}";
                Error.Handle(Except.ConnectionRefused, endPoint);
            }
            catch (IOException) // Connection lost
            {
                Error.Handle(Except.ConnectionLost, $"{Addr}");
            }
            catch (Exception ex) // Unhandled exception
            {
                throw ex;
            }
            finally // Free unmanaged resources
            {
                Dispose();
            }
        }

        /// Release any unmanaged resources
        public override void Dispose()
        {
            _listener?.Dispose();
            base.Dispose();
        }

        /// Bind the listener socket to an endpoint
        private void BindListener(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }

            _listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);
            try
            {
                _listener.Bind(endPoint);
            }
            catch (SocketException)
            {
                Dispose();
                Error.Handle(Except.SocketBind, $"{endPoint}");
            }
        }
    }
}
