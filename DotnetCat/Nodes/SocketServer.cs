using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Contracts;
using DotnetCat.Enums;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// SocketShell derived server node
    /// </summary>
    class SocketServer : SocketShell, IConnectable
    {
        private Socket _listener;

        /// Initialize new SocketServer
        public SocketServer() : base(address: IPAddress.Any)
        {
            _listener = null;
        }

        /// Listen for incoming TCP connections
        public override void Connect()
        {
            IPEndPoint remoteEP;
            BindListener(new IPEndPoint(Address, Port));

            try
            {
                _listener.Listen(1);
                Style.Status("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                if (Program.UsingShell)
                {
                    Executable ??= Cmd.GetDefaultShell(Platform);
                    bool hasStarted = StartProcess(Executable);

                    if (!hasStarted)
                    {
                        Error.Handle(ErrorType.ShellProcess, Executable);
                    }
                }

                remoteEP = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connected to {remoteEP}");

                base.Connect();
                WaitForExit();
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                {
                    string endPoint = $"{Address}:{Port}";
                    Error.Handle(ErrorType.ConnectionRefused, endPoint);
                }

                if (ex is IOException)
                {
                    Error.Handle(ErrorType.ConnectionLost, $"{Address}");
                }

                throw ex;
            }
            finally
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

            _listener = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp
            );

            try
            {
                _listener.Bind(endPoint);
            }
            catch (SocketException)
            {
                Dispose();
                Error.Handle(ErrorType.SocketBind, $"{endPoint}");
            }
        }
    }
}
