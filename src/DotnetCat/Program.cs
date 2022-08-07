using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DotnetCat.Errors;
using DotnetCat.Network.Nodes;
using DotnetCat.Utils;

namespace DotnetCat
{
    /// <summary>
    ///  Primary application startup object
    /// </summary>
    internal class Program
    {
        private static CmdLineArgs? _args;  // Command-line arguments

        private static Parser? _parser;     // Argument parser

        /// Local operating system
        public static Platform OS { get; private set; }

        /// Command-line arguments object
        public static CmdLineArgs Args
        {
            get => _args ??= new CmdLineArgs();
            set => _args = value;
        }

        /// Network socket node
        public static Node? SockNode { get; private set; }

        /// Original command-line arguments list
        public static List<string>? OrigArgs { get; private set; }

        /// Command-line argument parser
        private static Parser ArgParser
        {
            get => _parser ??= new Parser();
            set => _parser = value;
        }

        /// <summary>
        ///  Static application entry point
        /// </summary>
        public static void Main(string[] args)
        {
            Console.Title = $"DotnetCat ({Parser.Repo})";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS = Platform.Win;
            }
            else
            {
                OS = Platform.Nix;
            }

            // Display help information and exit
            if (args.IsNullOrEmpty() || Parser.NeedsHelp(args))
            {
                ArgParser.PrintHelp();
            }
            OrigArgs = args.ToList();

            InitializeNode(args);
            ConnectNode();

            Console.WriteLine();
            Environment.Exit(0);
        }

        /// <summary>
        ///  Parse the command-line arguments and initialize the socket node
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
            Args = ArgParser.Parse(args);

            SockNode = Args.Listen ? new ServerNode(Args) : new ClientNode(Args);
        }

        /// <summary>
        ///  Connect the TCP socket client or server node
        /// </summary>
        private static void ConnectNode()
        {
            if (SockNode is null)
            {
                throw new InvalidOperationException("Null socket node specified");
            }
            SockNode?.Connect();
        }
    }
}
