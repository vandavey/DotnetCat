using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using DotnetCat.Nodes;

namespace DotnetCat
{
    /// <summary>
    /// Primary application startup object
    /// </summary>
    internal class Program
    {
        /// Enable verbose console output
        public static bool Verbose => SockNode?.Verbose ?? false;

        /// Enable verbose exceptions
        public static bool Debug { get; set; }

        /// Using executable pipeline
        public static bool UsingExe { get; set; }

        /// Pipeline variant
        public static PipeType PipeVariant { get; set; }

        /// User-defined string payload
        public static string Payload { get; set; }

        /// Command-line arguments
        public static List<string> Args { get; set; }

        /// Network socket node
        public static Node SockNode { get; set; }

        /// Operating system
        public static Platform OS { get; private set; }

        /// File transfer option type
        public static TransferOpt Transfer { get; private set; }

        /// Original cmd-line arguments
        public static List<string> OrigArgs { get; private set; }

        /// <summary>
        /// Primary application entry point
        /// </summary>
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

            OrigArgs = args.ToList();

            // Display help info and exit
            if ((args.Length == 0) || Parser.NeedsHelp(args))
            {
                Parser.PrintHelp();
            }

            InitializeNode(args);
            ConnectNode();

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// <summary>
        /// Initialize node fields and properties
        /// </summary>
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
            Args = DefragArguments(args);

            List<string> lowerArgs = new();
            Args?.ForEach(arg => lowerArgs.Add(arg.ToLower()));

            int index;

            // Discard 'NoExit' cmd-line args options
            if ((index = lowerArgs.IndexOf("-noexit")) > -1)
            {
                Args.RemoveAt(index);
            }

            Transfer = GetTransferOpts();
            index = Parser.IndexOfFlag("--listen", 'l');

            // Determine if node is client/server
            if ((index > -1) || (Parser.IndexOfAlias('l') > -1))
            {
                SockNode = new ServerNode();
                return;
            }
            SockNode = new ClientNode();
        }

        /// <summary>
        /// Ensure string-literal arguments aren't fragmented
        /// </summary>
        private static List<string> DefragArguments(string[] args)
        {
            int delta = 0;
            List<string> list = args.ToList();

            // Get arguments starting with quote
            var query = from arg in args
                        let pos = Array.IndexOf(args, arg)
                        let quote = arg.FirstOrDefault()
                        let valid = arg.EndsWith(quote) && (arg.Length >= 2)
                        where arg.StartsWith("'")
                            || arg.StartsWith("\"")
                        select new { arg, pos, quote, valid };

            foreach (var item in query)
            {
                // Skip processed arguments
                if (delta > 0)
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
                if (eolQuery is null)
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
                list[listIndex] = defragged[1..(defragged.Length - 1)];
            }
            return list;
        }

        /// <summary>
        /// Get the file/socket communication operation type
        /// </summary>
        private static TransferOpt GetTransferOpts()
        {
            int outIndex = Parser.IndexOfFlag("--output", 'o');

            // Receive file data
            if ((outIndex > -1) || (Parser.IndexOfAlias('o') > -1))
            {
                return TransferOpt.Collect;
            }
            int sendIndex = Parser.IndexOfFlag("--send", 's');

            // Send file data
            if ((sendIndex > -1) || (Parser.IndexOfAlias('s') > -1))
            {
                return TransferOpt.Transmit;
            }
            return TransferOpt.None;
        }

        /// <summary>
        /// Parse arguments and initiate connection
        /// </summary>
        private static void ConnectNode()
        {
            Parser.ParseCharArgs();
            Parser.ParseFlagArgs();

            // Validate remaining cmd-line arguments
            switch (Args.Count)
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
                    Exception ex = null;

                    // Parse or resolve IP address
                    if (IPAddress.TryParse(Args[0], out IPAddress addr))
                    {
                        SockNode.Addr = addr;
                    }
                    else
                    {
                        (SockNode.Addr, ex) = Net.ResolveName(Args[0]);
                    }

                    SockNode.DestName = Args[0];

                    // Invalid destination host
                    if (SockNode.Addr is null)
                    {
                        Error.Handle(Except.InvalidAddr, Args[0], true, ex);
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
    }
}
