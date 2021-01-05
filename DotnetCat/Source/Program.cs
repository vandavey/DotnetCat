using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using DotnetCat.Nodes;

namespace DotnetCat
{
    /// <summary>
    /// Primary application startup opbject
    /// </summary>
    class Program
    {
        private static StyleHandler _style;

        private static ErrorHandler _error;

        private static ArgumentParser _parser;

        public static bool Verbose => SockNode?.Verbose ?? false;

        public static bool Recursive { get; set; }

        public static bool UsingExe { get; set; }

        public static Communicate FileComm { get; set; }

        public static Platform OS { get; set; }

        public static List<string> Args { get; set; }

        public static SocketNode SockNode { get; set; }

        /// Primary application entry point
        private static void Main(string[] args)
        {
            _parser = new ArgumentParser();
            OS = GetPlatform();

            // Display help info and exit
            if ((args.Count() == 0) || _parser.NeedsHelp(args))
            {
                _parser.PrintHelp();
            }

            InitializeNode(args);
            Connect();

            // Display exit message
            if (SockNode?.Verbose ?? Verbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// Determine if OS platform is Windows or Unix
        private static Platform GetPlatform()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Platform.Nix;
            }
            return Platform.Win;
        }

        /// Initialize node fields and properties
        private static void InitializeNode(string[] args)
        {
            _style ??= new StyleHandler();
            _error ??= new ErrorHandler();

            // Check for incomplete alias
            if (args.Contains("-"))
            {
                _error.Handle(Except.InvalidArgs, "-", true);
            }

            // Check for incomplete flag
            if (args.Contains("--"))
            {
                _error.Handle(Except.InvalidArgs, "--", true);
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
            FileComm = GetCommunication();

            // Determine if node is client/server
            if (GetNode() is Node.Server)
            {
                SockNode = new ServerNode();
                return;
            }
            SockNode = new ClientNode();
        }

        /// Parse arguments and initiate connection
        private static void Connect()
        {
            ParseCharArgs();
            ParseFlagArgs();

            // Validate remaining cmd-line arguments
            switch (Args.Count)
            {
                case 0: // Missing TARGET
                {
                    if (SockNode is ClientNode)
                    {
                        _error.Handle(Except.RequiredArgs, "TARGET", true);
                    }
                    break;
                }
                case 1: // Validate TARGET
                {
                    if (Args[0].StartsWith('-'))
                    {
                        _error.Handle(Except.UnknownArgs, Args[0], true);
                    }

                    if (!_parser.IsValidAddress(Args[0]).valid)
                    {
                        _error.Handle(Except.InvalidAddr, Args[0], true);
                    }
                    _parser.SetAddress(Args[0]);
                    break;
                }
                default: // Additional unexpected arguments
                {
                    string argsStr = string.Join(", ", Args);

                    if (Args[0].StartsWith('-'))
                    {
                        _error.Handle(Except.UnknownArgs, argsStr, true);
                    }
                    _error.Handle(Except.InvalidArgs, argsStr, true);
                    break;
                }
            }
            SockNode.Connect();
        }

        /// Determine the node type from the command line arguments
        private static Node GetNode()
        {
            int index = _parser.IndexOfFlag("--listen", 'l');

            if ((index > -1) || (_parser.IndexOfAlias('l') > -1))
            {
                return Node.Server;
            }
            return Node.Client;
        }

        /// Get the file/socket communication operation type
        private static Communicate GetCommunication()
        {
            int outIndex = _parser.IndexOfFlag("--output", 'o');

            // Receive file data
            if ((outIndex > -1) || (_parser.IndexOfAlias('o') > -1))
            {
                return Communicate.Collect;
            }
            int sendIndex = _parser.IndexOfFlag("--send", 's');

            // Send file data
            if ((sendIndex > -1) || (_parser.IndexOfAlias('s') > -1))
            {
                return Communicate.Transmit;
            }
            return Communicate.None;
        }

        /// Parse named arguments starting with one dash
        private static void ParseCharArgs()
        {
            // Locate all char flag arguments
            var query = from arg in Args.ToList()
                        let index = _parser.IndexOfFlag(arg)
                        where arg[0] == '-'
                            && arg[1] != '-'
                        select new { arg, index };

            foreach (var item in query)
            {
                if (item.arg.Contains('l'))
                    _parser.RemoveAlias(item.index, 'l');

                if (item.arg.Contains('v'))
                    _parser.SetVerbose(item.index, Argument.Alias);

                if (item.arg.Contains('r'))
                    _parser.SetRecurse(item.index, Argument.Alias);

                if (item.arg.Contains('p'))
                    _parser.SetPort(item.index, Argument.Alias);

                if (item.arg.Contains('e'))
                    _parser.SetExec(item.index, Argument.Alias);

                if (item.arg.Contains('o'))
                    _parser.SetCollect(item.index, Argument.Alias);

                if (item.arg.Contains('s'))
                    _parser.SetTransmit(item.index, Argument.Alias);

                if (_parser.ArgsValueAt(item.index) == "-")
                    Args.RemoveAt(_parser.IndexOfFlag("-"));
            }
        }

        /// Parse named arguments starting with two dashes
        private static void ParseFlagArgs()
        {
            // Locate all flag arguments
            var query = from arg in Args.ToList()
                        let index = _parser.IndexOfFlag(arg)
                        where arg.StartsWith("--")
                        select new { arg, index };

            foreach (var item in query)
            {
                switch (item.arg)
                {
                    case "--listen":
                        Args.RemoveAt(item.index);
                        break;
                    case "--verbose":
                        _parser.SetVerbose(item.index, Argument.Flag);
                        break;
                    case "--recurse":
                        _parser.SetRecurse(item.index, Argument.Flag);
                        break;
                    case "--port":
                        _parser.SetPort(item.index, Argument.Flag);
                        break;
                    case "--exec":
                        _parser.SetExec(item.index, Argument.Flag);
                        break;
                    case "--output":
                        _parser.SetCollect(item.index, Argument.Flag);
                        break;
                    case "--send":
                        _parser.SetTransmit(item.index, Argument.Flag);
                        break;
                }
            }
        }
    }
}
