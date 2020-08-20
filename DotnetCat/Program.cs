using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DotnetCat.Handlers;
using DotnetCat.Nodes;
using DotnetCat.Pipes;
using DotnetCat.Utils;

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

        public enum NodeType { Client, Server }

        public static bool IsUsingExec { get; set; }

        public static List<string> Args { get; set; }

        public static NodeAction SocketAction { get; set; }

        public static SocketShell SockShell { get; set; }

        public static Platform SysPlatform { get; } = GetPlatform();

        public static bool IsVerbose
        {
            get => SockShell?.IsVerbose ?? false;
        }

        /// Primary application entry point
        private static void Main(string[] args)
        {
            _parser = new ArgumentParser();

            if ((args.Count() == 0) || _parser.NeedsHelp(args))
            {
                _parser.PrintHelp();
            }

            InitializeNode(args);
            ConnectNode();

            if (SockShell?.IsVerbose ?? IsVerbose)
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
                return Platform.Unix;
            }

            return Platform.Windows;
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

            IsUsingExec = false;
            Args = args.ToList();

            List<string> lowerArgs = new List<string>();
            Args.ForEach(arg => lowerArgs.Add(arg.ToLower()));

            int index;

            if ((index = lowerArgs.IndexOf("-noexit")) > -1)
            {
                Args.RemoveAt(index);
            }

            SocketAction = GetNodeAction();

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
            ParseFlagArgs();
            ParseNamedArgs();

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
                    _error.Handle(ErrorType.InvalidAddress, Args[0], true);
                }

                _parser.SetAddress(Args[0]);
            }

            SockShell.Connect();
        }

        /// Determine the node type from the command line argumentss
        private static NodeType GetNodeType()
        {
            int index = _parser.IndexOfArgs("--listen", "-l");

            if ((index > -1) || (_parser.IndexOfFlag('l') > -1))
            {
                return NodeType.Server;
            }

            return NodeType.Client;
        }

        /// Determine if the user is tranferring files
        private static NodeAction GetNodeAction()
        {
            int recvIndex = _parser.IndexOfArgs("--recv", "-r");

            if ((recvIndex > -1) || (_parser.IndexOfFlag('r') > -1))
            {
                return NodeAction.RecvFile;
            }

            int sendIndex = _parser.IndexOfArgs("--send", "-s");

            if ((sendIndex > -1) || (_parser.IndexOfFlag('s') > -1))
            {
                return NodeAction.SendFile;
            }

            return NodeAction.None;
        }

        /// Parse named arguments starting with one dash
        private static void ParseFlagArgs()
        {
            var query = from arg in Args.ToList()
                        let index = _parser.IndexOfArgs(arg)
                        where arg[0] == '-'
                            && arg[1] != '-'
                        select new { arg, index };

            foreach (var item in query)
            {
                if (item.arg.Contains('l'))
                    _parser.UpdateArgs(item.index, 'l');

                if (item.arg.Contains('v'))
                    _parser.SetVerbose(item.index, ArgumentType.Flag);

                if (item.arg.Contains('p'))
                    _parser.SetPort(item.index, ArgumentType.Flag);

                if (item.arg.Contains('e'))
                    _parser.SetExec(item.index, ArgumentType.Flag);

                if (item.arg.Contains('r'))
                    _parser.SetRecv(item.index, ArgumentType.Flag);

                if (item.arg.Contains('s'))
                    _parser.SetSend(item.index, ArgumentType.Flag);

                if (_parser.ArgsValueAt(item.index) == "-")
                    Args.RemoveAt(_parser.IndexOfArgs("-"));
            }
        }

        /// Parse named arguments starting with two dashes
        private static void ParseNamedArgs()
        {
            var query = from arg in Args.ToList()
                        let index = _parser.IndexOfArgs(arg)
                        where arg.StartsWith("--")
                        select new { arg, index };

            foreach (var item in query)
            {
                switch (item.arg)
                {
                    case "--verbose":
                        _parser.SetVerbose(item.index, ArgumentType.Named);
                        break;
                    case "--port":
                        _parser.SetPort(item.index, ArgumentType.Named);
                        break;
                    case "--exec":
                        _parser.SetExec(item.index, ArgumentType.Named);
                        break;
                    case "--recv":
                        _parser.SetRecv(item.index, ArgumentType.Named);
                        break;
                    case "--send":
                        _parser.SetSend(item.index, ArgumentType.Named);
                        break;
                    case "--listen":
                        Args.RemoveAt(item.index);
                        break;
                }
            }
        }
    }
}
