using System;
using System.IO;
using DotnetCat.Contracts;
using DotnetCat.Enums;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// SocketShell derived client node
    /// </summary>
    class SocketClient : SocketShell, IConnectable
    {
        public SocketClient() : base()
        {
        }

        /// Connect to the specified IPv4 address and port number
        public override void Connect()
        {
            try
            {
                if (!Client.ConnectAsync(Address, Port).Wait(3500))
                {
                    throw new AggregateException();
                }

                NetStream = Client.GetStream();

                if (Program.UsingShell)
                {
                    bool hasStarted = StartProcess(
                        Executable ?? Cmd.GetDefaultShell(Platform)
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
                    Error.Handle(
                        ErrorType.ConnectionRefused,
                        $"{Address}:{Port}"
                    );
                }

                if (ex is IOException)
                {
                    Error.Handle(
                        ErrorType.ConnectionLost,
                        Address.ToString()
                    );
                }

                throw ex;
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
