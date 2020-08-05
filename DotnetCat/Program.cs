using DotnetCat.Handlers;
using DotnetCat.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetCat
{
    /// <summary>
    /// Primary application startup opbject
    /// </summary>
    class Program
    {
        private static StyleHandler _style;

        public static string Node { get; set; }

        public static string TransferType { get; set; }

        public static bool Verbose { get; set; }

        public static List<string> Args { get; set; }

        public static SocketServer Server { get; set; }
        
        public static SocketClient Client { get; set; }

        public static ArgumentParser Parser { get; set; }

        public static ErrorHandler Error { get; set; }

        public static bool IsUsingExec { get; set; }

        /// Primary application entry point
        private static void Main(string[] args)
        {
            Parser = new ArgumentParser();

            if ((args.Count() == 0) || Parser.NeedsHelp(args))
            {
                Parser.PrintHelp();
            }

            InitializeProperties(args);
            InitializeNode();

            if (Verbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// Initialize static properties and fields
        private static void InitializeProperties(string[] args)
        {
            _style = new StyleHandler();
            Error = new ErrorHandler();

            IsUsingExec = false;
            Verbose = false;

            int index;
            Args = args.ToList();

            List<string> lowerArgs = new List<string>();
            Args.ForEach(arg => lowerArgs.Add(arg.ToLower()));

            if ((index = lowerArgs.IndexOf("-noexit")) > -1)
            {
                Args.RemoveAt(index);
            }

            Node = GetNodeType();
            TransferType = GetTransferType();

            if (Node == "server")
            {
                Server = new SocketServer(TransferType);
            }
            else
            {
                Client = new SocketClient(TransferType);
            }
        }

        /// Parse command line arguments and initialize nodes
        private static void InitializeNode()
        {
            ParseShortArgs();
            ParseLongArgs();

            int size = Args.Count;

            if ((size == 0) && (Node != "server"))
            {
                Error.Handle("required", "TARGET", true);
            }

            if (size > 1)
            {
                string argsStr = string.Join(", ", Args);

                if (Args[0].StartsWith('-'))
                {
                    Error.Handle("unknown", argsStr, true);
                }

                Error.Handle("validation", argsStr, true);
            }

            if (size != 0)
            {
                if (!Parser.AddressIsValid(Args[0]))
                {
                    Error.Handle("address", Args[0], true);
                }

                Parser.SetAddress(Args[0]);
            }

            if (Node == "server")
            {
                Server.Listen();
            }
            else
            {
                Client.Connect();
            }
        }

        /// Determine the node type from the command line argumentss
        private static string GetNodeType()
        {
            int index = Parser.IndexOfArgs("--listen", "-l");

            if ((index > -1) || (Parser.IndexOfFlag('l') > -1))
            {
                return "server";
            }

            return "client";
        }

        /// Determine if the user is tranferring files
        private static string GetTransferType()
        {
            int recvIndex = Parser.IndexOfArgs("-r", "--recv");

            if ((recvIndex > -1) || (Parser.IndexOfFlag('r') > -1))
            {
                return "recv";
            }

            int sendIndex = Parser.IndexOfArgs("-s", "--send");

            if ((sendIndex > -1) || (Parser.IndexOfFlag('s') > -1))
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
                        let keyIndex = Parser.IndexOfArgs(arg)
                        let valIndex = keyIndex + 1
                        where arg.StartsWith('-')
                            && !arg.StartsWith("--")
                        select new { chars, keyIndex, valIndex };

            foreach (var item in query)
            {
                if (item.chars.Contains('l'))
                {
                    Parser.UpdateArgs(item.keyIndex, 'l');
                }

                if (item.chars.Contains('v'))
                {
                    Parser.SetVerbose();
                    Parser.UpdateArgs(item.keyIndex, 'v');
                    Verbose = true;
                }

                if (item.chars.Contains('p'))
                {
                    Parser.SetPort(Parser.ArgsValueAt(item.valIndex));
                    Parser.UpdateArgs(item.keyIndex, 'p');
                    Args.RemoveAt(item.valIndex);
                }

                if (item.chars.Contains('e'))
                {
                    Parser.SetExec(Parser.ArgsValueAt(item.valIndex));
                    Parser.UpdateArgs(item.keyIndex, 'e');
                    Args.RemoveAt(item.valIndex);
                    IsUsingExec = true;
                }

                if (item.chars.Contains('r'))
                {
                    Parser.SetFilePath(Parser.ArgsValueAt(item.valIndex));
                    Parser.UpdateArgs(item.keyIndex, 'r');
                    Args.RemoveAt(item.valIndex);
                }

                if (item.chars.Contains('s'))
                {
                    Parser.SetFilePath(Parser.ArgsValueAt(item.valIndex));
                    Parser.UpdateArgs(item.keyIndex, 'r');
                    Args.RemoveAt(item.valIndex);
                }

                if (Parser.ArgsValueAt(item.keyIndex) == "-")
                {
                    Args.RemoveAt(Parser.IndexOfArgs("-"));
                }
            }
        }

        /// Parse named arguments starting with two dashes
        private static void ParseLongArgs()
        {
            List<string> args = Args.ToList();

            var query = from arg in args
                        let keyName = arg.Replace("--", "")
                        let keyIndex = Parser.IndexOfArgs(arg)
                        let valIndex = keyIndex + 1
                        where arg.StartsWith("--")
                        select new { keyName, keyIndex, valIndex };

            foreach (var item in query)
            {
                if (item.keyName == "verbose")
                {
                    Parser.SetVerbose();
                    Args.RemoveAt(item.keyIndex);
                    Verbose = true;
                }
                else if (item.keyName == "port")
                {
                    Parser.SetPort(Parser.ArgsValueAt(item.valIndex));
                    Parser.RemoveNamedArg("port");
                }
                else if (item.keyName == "exec")
                {
                    Parser.SetExec(Parser.ArgsValueAt(item.valIndex));
                    Parser.RemoveNamedArg("exec");
                    IsUsingExec = true;
                }
                else if (item.keyName == "recv")
                {
                    Parser.SetFilePath(Parser.ArgsValueAt(item.valIndex));
                    Parser.RemoveNamedArg("recv");
                }
                else if (item.keyName == "send")
                {
                    Parser.SetFilePath(Parser.ArgsValueAt(item.valIndex));
                    Parser.RemoveNamedArg("send");
                }
                else if (item.keyName == "listen")
                {
                    Args.RemoveAt(item.keyIndex);
                }
            }
        }
    }
}
