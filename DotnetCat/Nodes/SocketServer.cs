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
            IPEndPoint remoteEP;
            BindListener(new IPEndPoint(Address, Port));

            try
            {
                _listener.Listen(1);
                Style.Status("Listening for incoming connections...");

                Client.Client = _listener.Accept();
                NetStream = Client.GetStream();

                if (Program.IsUsingExec)
                {
                    Executable ??= Cmd.GetDefaultShell(SysPlatform);
                    bool hasStarted = StartProcess(Executable);

                    if (!hasStarted)
                    {
                        Error.Handle("process", Executable);
                    }
                }

                remoteEP = Client.Client.RemoteEndPoint as IPEndPoint;
                Style.Status($"Connected to {remoteEP}");

                base.Connect();
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
        private void BindListener(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
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
                Close();
                Error.Handle("bind", endPoint.ToString());
            }
        }
    }
}
