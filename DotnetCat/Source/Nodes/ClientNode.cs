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
                        Dispose();
                        Error.Handle(Except.ExecProcess, Exe);
                    }
                }
                Style.Status($"Connected to {Addr}:{Port}");

                base.Connect();
                WaitForExit();
            }
            catch (AggregateException ex) // Connection refused
            {
                Dispose();
                Error.Handle(Except.ConnectionRefused, $"{Addr}:{Port}", ex);
            }
            catch (IOException ex) // Connection lost
            {
                Dispose();
                Error.Handle(Except.ConnectionLost, Addr.ToString(), ex);
            }
            Dispose();
        }
    }
}
