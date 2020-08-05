using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// SocketShell derived server node
    /// </summary>
    class SocketServer : SocketShell, ICloseable
    {
        private Socket _listener;

        /// Initialize new SocketServer
        public SocketServer(string tansferType) : base(tansferType)
        {
            _listener = null;
            this.Address = IPAddress.Any;
        }

        /// Listen for incoming TCP connections
        public void Listen()
        {
            IPEndPoint ipEP;
            BindListener(_listener = NewTcpSocket());

            try
            {
                _listener.Listen(1);
                Style.Status("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                if (Program.IsUsingExec)
                {
                    Shell ??= Cmd.GetDefaultShell();
                    bool hasStarted = StartProcess(Shell);

                    if (!hasStarted)
                    {
                        Error.Handle("process", Shell);
                    }
                }

                if (TransferType != null)
                {

                }

                ipEP = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connection established with {ipEP}");

                ConnectPipes();
                WaitForExit();
            }
            catch (Exception ex)
            {
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
