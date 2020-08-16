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

        private static SocketClient _client;
        private static SocketServer _server;

        private static ArgumentParser _parser;

        public enum NodeType { Client, Server }

        public static bool IsUsingExec { get; set; }

        public static List<string> Args { get; set; }

        public static NodeAction SocketAction { get; set; }

        public static SocketShell SockShell { get; set; }

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

            if (SockShell.IsVerbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// Initialize node fields and properties
        private static void InitializeNode(string[] args)
        {
            _style = new StyleHandler();
            _error = new ErrorHandler();

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
                SockShell = _server = new SocketServer();
            }
            else
            {
                SockShell = _client = new SocketClient();
            }
        }

        public static Platform GetPlatform()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Platform.Linux;
            }

            return Platform.Windows;
        }

        /// Parse arguments and initiate connection
        private static void ConnectNode()
        {
            ParseShortArgs();
            ParseLongArgs();

            int size = Args.Count;

            if ((size == 0) && (SockShell != _server))
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
                else if (!_parser.AddressIsValid(Args[0]))
                {
                    _error.Handle(ErrorType.InvalidAddress, Args[0], true);
                }
                SockShell = _parser.SetAddress(SockShell, Args[0]);
            }

            if (SockShell is SocketServer)
            {
                _server.Listen();
            }
            else
            {
                _client.Connect();
            }
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
        private static void ParseShortArgs()
        {
            var query = from arg in Args.ToList()
                        let argIndex = _parser.IndexOfArgs(arg)
                        let valIndex = argIndex + 1
                        where arg[0] == '-'
                            && arg[1] != '-'
                        select new { arg, argIndex, valIndex };

            foreach (var item in query)
            {
                if (item.arg.Contains('l'))
                    _parser.UpdateArgs(item.argIndex, 'l');

                if (item.arg.Contains('v'))
                    SetVerbose(item.argIndex, false);

                if (item.arg.Contains('p'))
                    SetPort(item.argIndex, item.valIndex, false);

                if (item.arg.Contains('e'))
                    SetExec(item.argIndex, item.valIndex, false);

                if (item.arg.Contains('r'))
                    SetRecv(item.argIndex, item.valIndex, false);

                if (item.arg.Contains('s'))
                    SetSend(item.argIndex, item.valIndex, false);

                if (_parser.ArgsValueAt(item.argIndex) == "-")
                    Args.RemoveAt(_parser.IndexOfArgs("-"));
            }
        }

        /// Parse named arguments starting with two dashes
        private static void ParseLongArgs()
        {
            var query = from arg in Args.ToList()
                        let argName = arg.Replace("--", "")
                        let argIndex = _parser.IndexOfArgs(arg)
                        let valIndex = argIndex + 1
                        where arg.StartsWith("--")
                        select new { argName, argIndex, valIndex };

            foreach (var item in query)
            {
                switch (item.argName)
                {
                    case "verbose":
                        SetVerbose(item.argIndex, true);
                        break;
                    case "port":
                        SetPort(item.argIndex, item.valIndex, true);
                        break;
                    case "exec":
                        SetExec(item.argIndex, item.valIndex, true);
                        break;
                    case "recv":
                        SetRecv(item.argIndex, item.valIndex, true);
                        break;
                    case "send":
                        SetSend(item.argIndex, item.valIndex, true);
                        break;
                    case "listen":
                        Args.RemoveAt(item.argIndex);
                        break;
                }
            }
        }

        /// Set the socket shell executable path
        private static void SetExec(int argIndex, int valIndex, bool isLong)
        {
            string exec = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetExec(SockShell, exec);

            if (isLong)
            {
                _parser.RemoveNamedArg("exec");
            }
            else
            {
                _parser.UpdateArgs(argIndex, 'e');
                Args.RemoveAt(valIndex);
            }

            IsUsingExec = true;
        }

        /// Set the socket shell port value
        private static void SetPort(int argIndex, int valIndex, bool isLong)
        {
            string port = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetPort(SockShell, port);

            if (isLong)
            {
                _parser.RemoveNamedArg("port");
                return;
            }

            _parser.UpdateArgs(argIndex, 'p');
            Args.RemoveAt(valIndex);
        }

        /// Set file tranfer type of socket shell to "recv"
        private static void SetRecv(int argIndex, int valIndex, bool isLong)
        {
            string path = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetFilePath(SockShell, path);

            if (isLong)
            {
                _parser.RemoveNamedArg("recv");
                return;
            }

            _parser.UpdateArgs(argIndex, 'r');
            Args.RemoveAt(valIndex);
        }

        /// Set file tranfer type of socket shell to "send"
        private static void SetSend(int argIndex, int valIndex, bool isLong)
        {
            string path = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetFilePath(SockShell, path);

            if (isLong)
            {
                _parser.RemoveNamedArg("send");
                return;
            }

            _parser.UpdateArgs(argIndex, 's');
            Args.RemoveAt(valIndex);
        }

        /// Enable verbose standard output
        private static void SetVerbose(int argIndex, bool isLong)
        {
            SockShell = _parser.SetVerbose(SockShell);

            if (isLong)
            {
                Args.RemoveAt(argIndex);
            }
            else
            {
                _parser.UpdateArgs(argIndex, 'v');
            }

            SockShell.IsVerbose = true;
        }
    }
}
