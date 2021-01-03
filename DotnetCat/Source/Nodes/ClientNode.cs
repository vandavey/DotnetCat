using System;
using System.IO;
using DotnetCat.Contracts;
using DotnetCat.Enums;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Client node for TCP socket connections
    /// </summary>
    class ClientNode : SocketNode, IConnectable
    {
        public ClientNode() : base()
        {
        }

        /// Connect to the specified IPv4 address and port number
        public override void Connect()
        {
            try
            {
                // Failed connection attempt
                if (!Client.ConnectAsync(Addr, Port).Wait(3500))
                {
                    throw new AggregateException();
                }
                NetStream = Client.GetStream();

                // Start executable process
                if (Program.UsingExe)
                {
                    bool hasStarted = Start(Exe ?? Cmd.GetDefaultExe(OS));

                    if (!hasStarted)
                    {
                        Error.Handle(Except.ShellProcess, Exe);
                    }
                }
                Style.Status($"Connected to {Addr}:{Port}");

                base.Connect();
                WaitForExit();
            }
            catch (AggregateException) // Connection refused
            {
                Error.Handle(Except.ConnectionRefused, $"{Addr}:{Port}");
            }
            catch (IOException) // Connection lost
            {
                Error.Handle(Except.ConnectionLost, Addr.ToString());
            }
            catch (Exception ex) // Unhandled exception
            {
                throw ex;
            }
            finally // Free unmanaged resources
            {
                base.Dispose();
            }
        }
    }
}
