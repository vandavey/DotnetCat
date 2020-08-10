using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// SocketShell derived server node
    /// </summary>
    class SocketServer : SocketShell, ICloseable
    {
        private Socket _listener;

        /// Initialize new SocketServer
        public SocketServer() : base(address: IPAddress.Any)
        {
            _listener = null;
        }

        /// Listen for incoming TCP connections
        public void Listen()
        {
            IPEndPoint endPoint;
            BindListener(_listener = CreateTcpSocket());

            try
            {
                _listener.Listen(1);
                Style.Status("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                if (Program.IsUsingExec)
                {
                    Executable ??= Cmd.GetDefaultShell();
                    bool hasStarted = StartProcess(Executable);

                    if (!hasStarted)
                    {
                        Error.Handle("process", Executable);
                    }
                }

                endPoint = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connected to {endPoint}");

                Connect();
                WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                if (ex is SocketException)
                {
                    Error.Handle("socket", $"{Address}:{Port}");
                }

                if (ex is IOException)
                {
                    Error.Handle("closed", Address.ToString());
                }

                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// Release any unmanaged resources
        public override void Close()
        {
            _listener?.Dispose();
            base.Close();
        }

        /// Bind the listener socket to an endpoint
        private void BindListener(Socket socket)
        {
            try
            {
                socket.Bind(new IPEndPoint(Address, Port));
            }
            catch (SocketException)
            {
                Close();
                Error.Handle("bind", $"{Address}:{Port}");
            }
        }
    }
}
