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

            List<string> lowArgs = args.Select(argument =>
            {
                return argument.ToLower();
            })
            .ToList();

            if ((index = lowArgs.IndexOf("-noexit")) > -1)
            {
                Args = Args.Where(argument =>
                {
                    return Array.IndexOf(args, argument) != index;
                })
                .ToList();
            }
            else
            {
                Args = args.ToList();
            }

            Node = NodeType();

            if (Node == "server")
            {
                Server = new SocketServer();
                Client = null;
            }
            else
            {
                Client = new SocketClient();
                Server = null;
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
        private static string NodeType()
        {
            int index = Parser.IndexOfArgs("--listen", "-l");

            if (index > -1)
            {
                return "server";
            }

            index = Parser.IndexOfFlag('l');
            return (index > -1) ? "server" : "client";
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
                else if (item.keyName == "listen")
                {
                    Args.RemoveAt(item.keyIndex);
                }
            }
        }
    }
}
