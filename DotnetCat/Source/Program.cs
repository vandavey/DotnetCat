using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using DotnetCat.Nodes;
using Error = DotnetCat.Handlers.ErrorHandler;

namespace DotnetCat
{
    /// <summary>
    /// Primary application startup opbject
    /// </summary>
    class Program
    {
        private static ArgumentParser _parser;

        public static bool Verbose => SockNode?.Verbose ?? false;

        public static bool Debug { get; set; }

        public static bool Recursive { get; set; }

        public static bool UsingExe { get; set; }

        public static Platform OS { get; set; }

        public static TransferOpt Transfer { get; set; }

        public static List<string> Args { get; set; }

        public static Node SockNode { get; set; }

        /// Primary application entry point
        private static void Main(string[] args)
        {
            _parser = new ArgumentParser();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS = Platform.Win;
            }
            else
            {
                OS = Platform.Nix;
            }

            // Display help info and exit
            if ((args.Count() == 0) || _parser.NeedsHelp(args))
            {
                _parser.PrintHelp();
            }

            InitializeNode(args);
            Connect();

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// Initialize node fields and properties
        private static void InitializeNode(string[] args)
        {
            // Check for incomplete alias
            if (args.Contains("-"))
            {
                Error.Handle(Except.InvalidArgs, "-", true);
            }

            // Check for incomplete flag
            if (args.Contains("--"))
            {
                Error.Handle(Except.InvalidArgs, "--", true);
            }

            UsingExe = false;
            Args = args.ToList();

            List<string> lowerArgs = new List<string>();
            Args?.ForEach(arg => lowerArgs.Add(arg.ToLower()));

            int index;

            // Discard 'NoExit' cmd-line args options
            if ((index = lowerArgs.IndexOf("-noexit")) > -1)
            {
                Args.RemoveAt(index);
            }
            
            Transfer = GetTransferOpts();
            index = _parser.IndexOfFlag("--listen", 'l');

            // Determine if node is client/server
            if ((index > -1) || (_parser.IndexOfAlias('l') > -1))
            {
                SockNode = new ServerNode();
                return;
            }
            SockNode = new ClientNode();
        }

        /// Get the file/socket communication operation type
        private static TransferOpt GetTransferOpts()
        {
            int outIndex = _parser.IndexOfFlag("--output", 'o');

            // Receive file data
            if ((outIndex > -1) || (_parser.IndexOfAlias('o') > -1))
            {
                return TransferOpt.Collect;
            }
            int sendIndex = _parser.IndexOfFlag("--send", 's');

            // Send file data
            if ((sendIndex > -1) || (_parser.IndexOfAlias('s') > -1))
            {
                return TransferOpt.Transmit;
            }
            return TransferOpt.None;
        }

        /// Parse arguments and initiate connection
        private static void Connect()
        {
            _parser.ParseCharArgs();
            _parser.ParseFlagArgs();

            // Validate remaining cmd-line arguments
            switch (Args.Count)
            {
                case 0: // Missing TARGET
                {
                    if (SockNode is ClientNode)
                    {
                        Error.Handle(Except.RequiredArgs, "TARGET", true);
                    }
                    break;
                }
                case 1: // Validate TARGET
                {
                    if (Args[0].StartsWith('-'))
                    {
                        Error.Handle(Except.UnknownArgs, Args[0], true);
                    }

                    bool isValid;
                    IPAddress addr = null;

                    try // Parse string as IP address
                    {
                        SockNode.Addr = IPAddress.Parse(Args[0]);
                        isValid = true;
                    }
                    catch (FormatException)
                    {
                        SockNode.Addr = ResolveHostName(Args[0]);
                        isValid = addr != null;
                    }

                    // Invalid destination host
                    if (!isValid)
                    {
                        Error.Handle(Except.InvalidAddr, Args[0], true);
                    }
                    break;
                }
                default: // Unexpected arguments
                {
                    string argsStr = string.Join(", ", Args);

                    if (Args[0].StartsWith('-'))
                    {
                        Error.Handle(Except.UnknownArgs, argsStr, true);
                    }
                    Error.Handle(Except.InvalidArgs, argsStr, true);
                    break;
                }
            }
            SockNode.Connect();
        }

        /// Resolve the IPv4 address of given hostname
        private static IPAddress ResolveHostName(string hostName)
        {
            IPHostEntry dnsAns;
            string machineName = Environment.MachineName;

            try // Resolve host name as IP address
            {
                dnsAns = Dns.GetHostEntry(hostName);

                if (dnsAns.AddressList.Contains(IPAddress.Loopback))
                {
                    return IPAddress.Loopback;
                }
            }
            catch (SocketException) // No DNS entries found
            {
                return null;
            }

            if (dnsAns.HostName.ToLower() != machineName.ToLower())
            {
                foreach (IPAddress addr in dnsAns.AddressList)
                {
                    if (addr.AddressFamily is AddressFamily.InterNetwork)
                    {
                        return addr;
                    }
                }
                return null;
            }

            Socket socket = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Dgram,
                                       ProtocolType.Udp);
            // Get local address
            using (socket)
            {
                socket.Connect("8.8.8.8", 53);
                return (socket.LocalEndPoint as IPEndPoint).Address;
            }
        }
    }
}
