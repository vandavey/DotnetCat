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

        public static bool Recursive { get; set; }

        public static bool UsingShell { get; set; }

        public static List<string> Args { get; set; }

        public static IOActionType IOAction { get; set; }

        public static PlatformType Platform { get; set; }

        public static SocketShell SockShell { get; set; }

        public static bool Verbose
        {
            get => SockShell?.Verbose ?? false;
        }

        /// Primary application entry point
        private static void Main(string[] args)
        {
            _parser = new ArgumentParser();
            Platform = GetPlatform();

            if ((args.Count() == 0) || _parser.NeedsHelp(args))
            {
                _parser.PrintHelp();
            }

            InitializeNode(args);
            ConnectNode();

            if (SockShell?.Verbose ?? Verbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// Determine if OS platform is Windows or Unix
        private static PlatformType GetPlatform()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return PlatformType.Unix;
            }

            return PlatformType.Windows;
        }

        /// Initialize node fields and properties
        private static void InitializeNode(string[] args)
        {
            _style = new StyleHandler();
            _error = new ErrorHandler();

            if (args.Contains("-"))
            {
                _error.Handle(ErrorType.ArgValidation, "-", true);
            }
            else if (args.Contains("--"))
            {
                _error.Handle(ErrorType.ArgValidation, "--", true);
            }

            UsingShell = false;
            Args = args.ToList();

            List<string> lowerArgs = new List<string>();
            Args.ForEach(arg => lowerArgs.Add(arg.ToLower()));

            int index;

            if ((index = lowerArgs.IndexOf("-noexit")) > -1)
            {
                Args.RemoveAt(index);
            }

            IOAction = GetIOAction();

            if (GetNodeType() == NodeType.Server)
            {
                SockShell = new SocketServer();
            }
            else
            {
                SockShell = new SocketClient();
            }
        }

        /// Parse arguments and initiate connection
        private static void ConnectNode()
        {
            ParseCharArgs();
            ParseFlagArgs();

            int size = Args.Count;

            if ((size == 0) && (SockShell is SocketClient))
            {
                _error.Handle(ErrorType.RequiredArg, "TARGET", true);
            }

            if (size > 1)
            {
                string argsStr = string.Join(", ", Args);

                if (Args[0].StartsWith('-'))
                {
                    _error.Handle(ErrorType.UnknownArg, argsStr, true);
                }
                _error.Handle(ErrorType.ArgValidation, argsStr, true);
            }

            if (size == 1)
            {
                if (Args[0].StartsWith('-'))
                {
                    _error.Handle(ErrorType.UnknownArg, Args[0], true);
                }
                else if (!_parser.IsValidAddress(Args[0]).valid)
                {
                    _error.Handle(ErrorType.InvalidAddr, Args[0], true);
                }

                _parser.SetAddress(Args[0]);
            }

            SockShell.Connect();
        }

        /// Determine the node type from the command line argumentss
        private static NodeType GetNodeType()
        {
            int index = _parser.IndexOfFlag("--listen", 'l');

            if ((index > -1) || (_parser.IndexOfAlias('l') > -1))
            {
                return NodeType.Server;
            }

            return NodeType.Client;
        }

        /// Determine if the user is tranferring files
        private static IOActionType GetIOAction()
        {
            int outIndex = _parser.IndexOfFlag("--output", 'o');

            if ((outIndex > -1) || (_parser.IndexOfAlias('o') > -1))
            {
                return IOActionType.WriteFile;
            }

            int sendIndex = _parser.IndexOfFlag("--send", 's');

            if ((sendIndex > -1) || (_parser.IndexOfAlias('s') > -1))
            {
                return IOActionType.ReadFile;
            }

            return IOActionType.None;
        }

        /// Parse named arguments starting with one dash
        private static void ParseCharArgs()
        {
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
                    _parser.SetVerbose(item.index, ArgumentType.Alias);

                if (item.arg.Contains('r'))
                    _parser.SetRecurse(item.index, ArgumentType.Alias);

                if (item.arg.Contains('p'))
                    _parser.SetPort(item.index, ArgumentType.Alias);

                if (item.arg.Contains('e'))
                    _parser.SetExec(item.index, ArgumentType.Alias);

                if (item.arg.Contains('o'))
                    _parser.SetOutput(item.index, ArgumentType.Alias);

                if (item.arg.Contains('s'))
                    _parser.SetSend(item.index, ArgumentType.Alias);

                if (_parser.ArgsValueAt(item.index) == "-")
                    Args.RemoveAt(_parser.IndexOfFlag("-"));
            }
        }

        /// Parse named arguments starting with two dashes
        private static void ParseFlagArgs()
        {
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
                        _parser.SetVerbose(item.index, ArgumentType.Flag);
                        break;
                    case "--recurse":
                        _parser.SetRecurse(item.index, ArgumentType.Flag);
                        break;
                    case "--port":
                        _parser.SetPort(item.index, ArgumentType.Flag);
                        break;
                    case "--exec":
                        _parser.SetExec(item.index, ArgumentType.Flag);
                        break;
                    case "--output":
                        _parser.SetOutput(item.index, ArgumentType.Flag);
                        break;
                    case "--send":
                        _parser.SetSend(item.index, ArgumentType.Flag);
                        break;
                }
            }
        }
    }
}
