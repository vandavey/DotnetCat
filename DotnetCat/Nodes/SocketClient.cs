using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Utils;

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
                if (!Client.ConnectAsync(Address, Port).Wait(3000))
                {
                    throw new AggregateException();
                }

                NetStream = Client.GetStream();

                if (Program.IsUsingExec)
                {
                    bool hasStarted = StartProcess(
                        Executable ?? Cmd.GetDefaultShell(SysPlatform)
                    );

                    if (!hasStarted)
                    {
                        Error.Handle(ErrorType.ShellProcess, Executable);
                    }
                }

                Style.Status($"Connected to {Address}:{Port}");

                base.Connect();
                WaitForExit();
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    Error.Handle(ErrorType.ConnectionRefused, $"{Address}:{Port}");
                }

                if (ex is IOException)
                {
                    Error.Handle(ErrorType.ConnectionLost, $"{Address}");
                }

                throw ex;
            }
            finally
            {
                base.Close();
            }
        }
    }
}
