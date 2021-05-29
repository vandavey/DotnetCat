using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;

namespace DotnetCat.Nodes
{
    /// <summary>
    /// Client node for TCP socket connections
    /// </summary>
    class ClientNode : Node, ISockErrorHandled
    {
        private IPEndPoint _ep;  // Remote endpoint

        /// <summary>
        /// Initialize object
        /// </summary>
        public ClientNode() : base() => _ep = default;

        /// <summary>
        /// Cleanup resources
        /// </summary>
        ~ClientNode() => Dispose();

        /// <summary>
        /// Connect to the specified IPv4 address and port number
        /// </summary>
        public override void Connect()
        {
            _ = Addr ?? throw new ArgumentNullException(nameof(Addr));
            _ep ??= new IPEndPoint(Addr, Port);

            try  // Connect with timeout
            {
                if (!Client.ConnectAsync(Addr, Port).Wait(3500))
                {
                    throw new SocketException(10060);  // Socket timeout
                }
                NetStream = Client.GetStream();

                // Start executable process
                if (Program.UsingExe)
                {
                    if (!Start(Exe ??= Command.GetDefaultExe(OS)))
                    {
                        PipeError(Except.ExeProcess, Exe);
                    }
                }
                Style.Info($"Connected to {_ep}");

                base.Connect();
                WaitForExit();

                // Connection closed status
                Style.Info($"Connection to {_ep.Address} closed");
            }
            catch (AggregateException ex)  // Connection refused
            {
                PipeError(Except.ConnectionRefused, _ep, ex, Level.Warn);
            }
            catch (SocketException ex)     // Error (likely timeout)
            {
                PipeError(Except.ConnectionTimeout, _ep, ex, Level.Warn);
            }
            catch (IOException ex)         // Connection lost
            {
                PipeError(Except.ConnectionLost, Addr.ToString(), ex);
            }

            Dispose();
        }
    }
}
