using System;
using System.IO;
using System.Net.Sockets;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// SocketShell derived client node
    /// </summary>
    class SocketClient : SocketShell, ICloseable
    {
        public SocketClient() : base()
        {
        }

        /// Connect to the specified IPv4 address and port number
        public override void Connect()
        {
            try
            {
                Client.Connect(Address, Port);
                NetStream = Client.GetStream();

                if (Program.IsUsingExec)
                {
                    bool hasStarted = StartProcess(
                        Executable ?? Cmd.GetDefaultShell()
                    );

                    if (!hasStarted)
                    {
                        Error.Handle("process", Executable);
                    }
                }

                Style.Status($"Connected to {Address}:{Port}");

                base.Connect();
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
