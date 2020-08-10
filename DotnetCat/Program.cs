using System;
using System.Collections.Generic;
using System.Linq;
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

        private static SocketClient _client;
        private static SocketServer _server;

        private static ArgumentParser _parser;

        public static bool IsUsingExec { get; set; }

        public static string TransferType { get; set; }

        public static List<string> Args { get; set; }

        public static SocketShell SockShell { get; set; }

        public static string Usage { get => _parser.UsageText; }

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

            int index;
            Args = args.ToList();

            List<string> lowerArgs = new List<string>();
            Args.ForEach(arg => lowerArgs.Add(arg.ToLower()));

            if ((index = lowerArgs.IndexOf("-noexit")) > -1)
            {
                Args.RemoveAt(index);
            }

            string nodeType = GetNodeType();
            TransferType = GetTransferType();

            if (nodeType == "server")
            {
                SockShell = _server = new SocketServer();
            }
            else
            {
                SockShell = _client = new SocketClient();
            }
        }

        /// Parse arguments and initiate connection
        private static void ConnectNode()
        {
            ParseShortArgs();
            ParseLongArgs();

            int size = Args.Count;

            if ((size == 0) && (SockShell != _server))
            {
                _error.Handle("required", "TARGET", true);
            }

            if (size > 1)
            {
                string argsStr = string.Join(", ", Args);

                if (Args[0].StartsWith('-'))
                {
                    _error.Handle("unknown", argsStr, true);
                }

                _error.Handle("validation", argsStr, true);
            }

            if (size != 0)
            {
                if (!_parser.AddressIsValid(Args[0]))
                {
                    _error.Handle("address", Args[0], true);
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
        private static string GetNodeType()
        {
            int index = _parser.IndexOfArgs("--listen", "-l");

            if ((index > -1) || (_parser.IndexOfFlag('l') > -1))
            {
                return "server";
            }

            return "client";
        }

        /// Determine if the user is tranferring files
        private static string GetTransferType()
        {
            int recvIndex = _parser.IndexOfArgs("-r", "--recv");

            if ((recvIndex > -1) || (_parser.IndexOfFlag('r') > -1))
            {
                return "recv";
            }

            int sendIndex = _parser.IndexOfArgs("-s", "--send");

            if ((sendIndex > -1) || (_parser.IndexOfFlag('s') > -1))
            {
                return "send";
            }

            return null;
        }

        /// Parse named arguments starting with one dash
        private static void ParseShortArgs()
        {
            List<string> args = Args.ToList();

            var query = from arg in args
                        let chars = arg.ToList()
                        let keyIndex = _parser.IndexOfArgs(arg)
                        let valIndex = keyIndex + 1
                        where arg.StartsWith('-')
                            && !arg.StartsWith("--")
                        select new { chars, keyIndex, valIndex };

            foreach (var item in query)
            {
                if (item.chars.Contains('l'))
                {
                    _parser.UpdateArgs(item.keyIndex, 'l');
                }

                if (item.chars.Contains('v'))
                {
                    SetVerbose(item.keyIndex, false);
                }

                if (item.chars.Contains('p'))
                {
                    SetPort(item.keyIndex, item.valIndex, false);
                }

                if (item.chars.Contains('e'))
                {
                    SetExec(item.keyIndex, item.valIndex, false);
                }

                if (item.chars.Contains('r'))
                {
                    SetRecv(item.keyIndex, item.valIndex, false);
                }

                if (item.chars.Contains('s'))
                {
                    SetSend(item.keyIndex, item.valIndex, true);
                }

                if (_parser.ArgsValueAt(item.keyIndex) == "-")
                {
                    Args.RemoveAt(_parser.IndexOfArgs("-"));
                }
            }
        }

        /// Parse named arguments starting with two dashes
        private static void ParseLongArgs()
        {
            List<string> args = Args.ToList();

            var query = from arg in args
                        let keyName = arg.Replace("--", "")
                        let keyIndex = _parser.IndexOfArgs(arg)
                        let valIndex = keyIndex + 1
                        where arg.StartsWith("--")
                        select new { keyName, keyIndex, valIndex };

            foreach (var item in query)
            {
                if (item.keyName == "verbose")
                {
                    SetVerbose(item.keyIndex, true);
                }
                else if (item.keyName == "port")
                {
                    SetPort(item.keyIndex, item.valIndex, true);
                }
                else if (item.keyName == "exec")
                {
                    SetExec(item.keyIndex, item.valIndex, true);
                }
                else if (item.keyName == "recv")
                {
                    SetRecv(item.keyIndex, item.valIndex, true);
                }
                else if (item.keyName == "send")
                {
                    SetSend(item.keyIndex, item.valIndex, true);
                }
                else if (item.keyName == "listen")
                {
                    Args.RemoveAt(item.keyIndex);
                }
            }
        }

        /// Set the socket shell executable path
        private static void SetExec(int keyIndex, int valIndex, bool isLong)
        {
            string exec = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetExec(SockShell, exec);

            if (isLong)
            {
                _parser.RemoveNamedArg("exec");
            }
            else
            {
                _parser.UpdateArgs(keyIndex, 'e');
                Args.RemoveAt(valIndex);
            }

            IsUsingExec = true;
        }

        /// Set the socket shell port value
        private static void SetPort(int keyIndex, int valIndex, bool isLong)
        {
            string port = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetPort(SockShell, port);

            if (isLong)
            {
                _parser.RemoveNamedArg("port");
                return;
            }

            _parser.UpdateArgs(keyIndex, 'p');
            Args.RemoveAt(valIndex);
        }

        /// Set file tranfer type of socket shell to "recv"
        private static void SetRecv(int keyIndex, int valIndex, bool isLong)
        {
            string path = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetFilePath(SockShell, path);

            if (isLong)
            {
                _parser.RemoveNamedArg("recv");
                return;
            }

            _parser.UpdateArgs(keyIndex, 'r');
            Args.RemoveAt(valIndex);
        }

        /// Set file tranfer type of socket shell to "send"
        private static void SetSend(int keyIndex, int valIndex, bool isLong)
        {
            string path = _parser.ArgsValueAt(valIndex);
            SockShell = _parser.SetFilePath(SockShell, path);

            if (isLong)
            {
                _parser.RemoveNamedArg("send");
                return;
            }

            _parser.UpdateArgs(keyIndex, 's');
            Args.RemoveAt(valIndex);
        }

        /// Enable verbose standard output
        private static void SetVerbose(int keyIndex, bool isLong)
        {
            SockShell = _parser.SetVerbose(SockShell);

            if (isLong)
            {
                Args.RemoveAt(keyIndex);
            }
            else
            {
                _parser.UpdateArgs(keyIndex, 'v');
            }

            SockShell.IsVerbose = true;
        }
    }
}
