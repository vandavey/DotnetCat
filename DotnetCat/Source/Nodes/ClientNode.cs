using System;
using System.IO;
using DotnetCat.Contracts;
using DotnetCat.Enums;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Client node for TCP socket connections
    /// </summary>
    class ClientNode : Node, IErrorHandled
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
                        PipeError(Except.ExecProcess, Exe);
                    }
                }
                Style.Status($"Connected to {Addr}:{Port}");

                base.Connect();
                WaitForExit();
            }
            catch (AggregateException ex) // Connection refused
            {
                PipeError(Except.ConnectionRefused, $"{Addr}:{Port}", ex);
            }
            catch (IOException ex) // Connection lost
            {
                PipeError(Except.ConnectionLost, Addr.ToString(), ex);
            }
            Dispose();
        }
    }
}
