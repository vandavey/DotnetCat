using System;
using System.IO;
using System.Net.Sockets;

namespace DotnetCat
{
    /// <summary>
    /// SocketShell derived client node
    /// </summary>
    class SocketClient : SocketShell, ICloseable
    {
        public SocketClient(string tansferType) : base(tansferType)
        {
        }

        /// Connect to the specified IPv4 address and port number
        public void Connect()
        {
            try
            {
                Client.Connect(Address, Port);
                NetStream = Client.GetStream();

                if (Program.IsUsingExec)
                {
                    bool hasStarted = StartProcess(
                        Shell ?? Cmd.GetDefaultShell()
                    );

                    if (!hasStarted)
                    {
                        Error.Handle("process", Shell);
                    }
                }

                Style.Status($"Connected to {Address}:{Port}");

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
    }
}
