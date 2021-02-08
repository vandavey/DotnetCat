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
    /// Primary application startup object
    /// </summary>
    class Program
    {
        private static ArgumentParser _parser;

        public static bool Verbose => SockNode?.Verbose ?? false;

        public static bool Debug { get; set; }

        public static bool UsingExe { get; set; }

        public static PipeType PipeVariant { get; set; }

        public static string Payload { get; set; }

        public static List<string> Args { get; set; }

        public static Node SockNode { get; set; }

        public static Platform OS { get; private set; }

        public static TransferOpt Transfer { get; private set; }

        /// Primary application entry point
        private static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS = Platform.Win;
            }
            else
            {
                OS = Platform.Nix;
            }

            _parser = new ArgumentParser();

            // Display help info and exit
            if ((args.Count() == 0) || _parser.NeedsHelp(args))
            {
                _parser.PrintHelp();
            }

            InitializeNode(args);
            ConnectNode();

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
            Args = DefragArgs(args);

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

        /// Ensure string-literal arguments aren't fragmented
        private static List<string> DefragArgs(string[] args)
        {
            int delta = 0;
            List<string> list = args.ToList();

            // Get arguments starting with quote
            var query = from arg in args
                        let pos = Array.IndexOf(args, arg)
                        let quote = arg.FirstOrDefault()
                        let valid = arg.EndsWith(quote) && arg.Length >= 2
                        where arg.StartsWith("'")
                            || arg.StartsWith("\"")
                        select new { arg, pos, quote, valid };

            foreach (var item in query)
            {
                if (delta > 0)  // Skip processed arguments
                {
                    delta -= 1;
                    continue;
                }
                int listIndex = list.IndexOf(item.arg);

                // Non-fragmented string
                if (item.valid)
                {
                    list[listIndex] = item.arg[1..(item.arg.Length - 1)];
                    continue;
                }

                // Get argument containing string EOL
                var eolQuery = (from arg in args
                                let pos = Array.IndexOf(args, arg, item.pos + 1)
                                where pos > item.pos
                                    && (arg == item.quote.ToString()
                                        || arg.EndsWith(item.quote))
                                select new { arg, pos }).FirstOrDefault();

                // Missing EOL (quote)
                if (eolQuery == null)
                {
                    Error.Handle(Except.StringEOL,
                                 string.Join(", ", args[item.pos..]), true);
                }

                delta = eolQuery.pos - item.pos;
                int endIndex = item.pos + delta;

                // Append fragments and remove duplicates
                for (int i = item.pos + 1; i < endIndex + 1; i++)
                {
                    list[listIndex] += $" {args[i]}";
                    list.Remove(args[i]);
                }

                string defragged = list[listIndex];
                list[listIndex] = defragged[1..(defragged.Count() - 1)];
            }
            return list;
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
        private static void ConnectNode()
        {
            _parser.ParseCharArgs();
            _parser.ParseFlagArgs();

            // Validate remaining cmd-line arguments
            switch (Args.Count())
            {
                case 0:   // Missing TARGET
                {
                    if (SockNode is ClientNode)
                    {
                        Error.Handle(Except.RequiredArgs, "TARGET", true);
                    }
                    break;
                }
                case 1:   // Validate TARGET
                {
                    if (Args[0].StartsWith('-'))
                    {
                        Error.Handle(Except.UnknownArgs, Args[0], true);
                    }

                    try  // Parse string as IP address
                    {
                        SockNode.Addr = IPAddress.Parse(Args[0]);
                    }
                    catch (FormatException)
                    {
                        SockNode.Addr = ResolveHostName(Args[0]);
                    }

                    // Invalid destination host
                    if (SockNode.Addr == null)
                    {
                        Error.Handle(Except.InvalidAddr, Args[0], true);
                    }
                    break;
                }
                default:  // Unexpected arguments
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

            try  // Resolve host name as IP address
            {
                dnsAns = Dns.GetHostEntry(hostName);

                if (dnsAns.AddressList.Contains(IPAddress.Loopback))
                {
                    return IPAddress.Loopback;
                }
            }
            catch (SocketException)  // No DNS entries found
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

            using Socket socket = new Socket(AddressFamily.InterNetwork,
                                             SocketType.Dgram,
                                             ProtocolType.Udp);
            // Get active local IP address
            socket.Connect("8.8.8.8", 53);
            return (socket.LocalEndPoint as IPEndPoint).Address;
        }
    }
}
