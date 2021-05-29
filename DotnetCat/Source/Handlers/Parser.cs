using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCat.Enums;
using DotnetCat.Nodes;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Command line argument parser and validator
    /// </summary>
    class Parser
    {
        private readonly string _appTitle;  // Application title
        private readonly string _help;      // Help information

        /// <summary>
        /// Initialize object
        /// </summary>
        public Parser()
        {
            _appTitle = (OS is Platform.Nix) ? "dncat" : "dncat.exe";
            _help = GetHelp(_appTitle, GetUsage(_appTitle));
        }

        /// Local operating system
        private static Platform OS => Program.OS;

        /// Enable verbose exceptions
        private static bool Debug
        {
            set => Program.Debug = value;
        }

        /// Pipeline type (enum variant)
        private static PipeType PipeVariant
        {
            set => Program.PipeVariant = value;
        }

        /// User-defined string payload
        private static string Payload
        {
            set => Program.Payload = value;
        }

        /// Command-line arguments
        private static List<string> Args
        {
            get => Program.Args;
            set => Program.Args = value;
        }

        /// Network node
        private static Node SockNode
        {
            get => Program.SockNode;
            set => Program.SockNode = value;
        }

        /// <summary>
        /// Get the application usage string
        /// </summary>
        public static string GetUsage(string appTitle = "dncat")
        {
            return $"Usage: {appTitle} [OPTIONS] TARGET";
        }

        /// <summary>
        /// Check for help flag in cmd-line arguments
        /// </summary>
        public static bool NeedsHelp(string[] args)
        {
            // Count matching arguments
            int count = (from arg in args
                         where arg.ToLower() == "--help"
                             || (arg.Length > 1
                                 && arg[0] == '-'
                                 && arg[1] != '-'
                                 && (arg.Contains('h')
                                     || arg.Contains('?')))
                         select arg).Count();

            return count > 0;
        }

        /// <summary>
        /// Get index of cmd-line argument with the specified char
        /// </summary>
        public static int IndexOfAlias(char alias)
        {
            // Query cmd-line arguments
            List<int> query = (from arg in Args
                               where arg.Contains(alias)
                                   && arg[0] == '-'
                                   && arg[1] != '-'
                               select Args.IndexOf(arg)).ToList();

            return (query.Count > 0) ? query[0] : -1;
        }

        /// <summary>
        /// Get the matching cmd-line argument index
        /// </summary>
        public static int IndexOfFlag(string flag, char? alias = default)
        {
            if (flag == "-")
            {
                return Args.IndexOf(flag);
            }

            // Assign argument alias
            if (alias.ToString() is not null or "")
            {
                foreach (char ch in alias.ToString())
                {
                    if (char.IsLetter(ch))
                    {
                        alias = ch;
                    }
                }
            }

            // Query cmd-line arguments
            List<int> query = (from arg in Args
                               where arg.ToLower() == flag.ToLower()
                                   || arg == $"-{alias}"
                               select Args.IndexOf(arg)).ToList();

            return (query.Count > 0) ? query[0] : -1;
        }

        /// <summary>
        /// Print application help message to console output
        /// </summary>
        public void PrintHelp()
        {
            Console.WriteLine(_help);
            Environment.Exit(0);
        }

        /// <summary>
        /// Parse named arguments starting with one dash
        /// </summary>
        public void ParseCharArgs()
        {
            // Locate all char flag arguments
            var query = from arg in Args.ToList()
                        let index = IndexOfFlag(arg)
                        where arg.Length >= 2
                            && arg[0] == '-'
                            && arg[1] != '-'
                        select new { arg, index };

            foreach (var item in query)
            {
                if (item.arg.Contains('l'))  // Listen for connection
                {
                    RemoveAlias(item.index, 'l');
                }

                if (item.arg.Contains('v'))  // Verbose output
                {
                    SockNode.Verbose = true;
                    RemoveAlias(item.index, 'v');
                }

                if (item.arg.Contains('d'))  // Debug output
                {
                    Debug = SockNode.Verbose = true;
                    RemoveAlias(item.index, 'd');
                }

                if (item.arg.Contains('p'))  // Connection port
                {
                    SockNode.Port = GetPort(item.index);
                    RemoveAlias(item.index, 'p', remValue: true);
                }

                if (item.arg.Contains('e'))  // Executable path
                {
                    SockNode.Exe = GetExecutable(item.index);
                    RemoveAlias(item.index, 'e', remValue: true);
                }

                if (item.arg.Contains('o'))  // Receive file data
                {
                    SockNode.FilePath = GetTransfer(item.index);
                    RemoveAlias(item.index, 'o', remValue: true);
                }

                if (item.arg.Contains('s'))  // Send file data
                {
                    SockNode.FilePath = GetTransfer(item.index);
                    RemoveAlias(item.index, 's', remValue: true);
                }

                if (item.arg.Contains('t'))  // Send string data
                {
                    Payload = GetText(item.index);
                    RemoveAlias(item.index, 't', remValue: true);
                }

                if (ArgsValueAt(item.index) == "-")
                {
                    Args.RemoveAt(IndexOfFlag("-"));
                }
            }
        }

        /// <summary>
        /// Parse named arguments starting with two dashes
        /// </summary>
        public void ParseFlagArgs()
        {
            // Locate all flag arguments
            var query = from arg in Args.ToList()
                        let index = IndexOfFlag(arg)
                        where arg.StartsWith("--")
                        select new { arg, index };

            foreach (var item in query)
            {
                switch (item.arg)
                {
                    case "--listen":   // Listen for connection
                    {
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--verbose":  // Verbose output
                    {
                        SockNode.Verbose = true;
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--debug":    // Debug output
                    {
                        Debug = SockNode.Verbose = true;
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--port":     // Connection port
                    {
                        SockNode.Port = GetPort(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--exec":     // Executable path
                    {
                        SockNode.Exe = GetExecutable(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--text":     // Send string data
                    {
                        Payload = GetText(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--output":   // Receive file data
                    case "--send":     // Send file data
                    {
                        SockNode.FilePath = GetTransfer(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get application help message as a string
        /// </summary>
        private static string GetHelp(string appTitle, string appUsage)
        {
            string lf = Environment.NewLine;

            return string.Join(lf, new string[]
            {
                "DotnetCat (https://github.com/vandavey/DotnetCat)",
                $"{appUsage}{lf}",
                $"Remote command shell application{lf}",
                "Positional Arguments:",
                $"  TARGET                    Remote/local IPv4 address{lf}",
                "Optional Arguments:",
                "  -h/-?,   --help           Show this help message and exit",
                "  -v,      --verbose        Enable verbose console output",
                "  -d,      --debug          Output verbose error information",
                "  -l,      --listen         Listen for incoming connections",
                "  -p PORT, --port PORT      Specify port to use for endpoint.",
                "                            (Default: 4444)",
                "  -e EXEC, --exec EXEC      Executable process file path",
                "  -o PATH, --output PATH    Receive file from remote host",
                "  -s PATH, --send PATH      Send local file or folder",
                $"  -t DATA, --text DATA      Send string to remote host{lf}",
                "Usage Examples:",
                $"  {appTitle} -le powershell.exe",
                $"  {appTitle} 10.0.0.152 -p 4444 localhost",
                $"  {appTitle} -ve /bin/bash 192.168.1.9{lf}",
            });
        }

        /// <summary>
        /// Remove character (alias) from a cmd-line argument
        /// </summary>
        private static void RemoveAlias(int index,
                                        char alias,
                                        bool remValue = false) {
            // Invalid index received
            if (!ValidIndex(index) || (remValue && !ValidIndex(index + 1)))
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            Args[index] = Args[index].Replace(alias.ToString(), string.Empty);

            // Remove arg value if requested
            if (remValue)
            {
                Args.RemoveAt(index + 1);
            }
        }

        /// <summary>
        /// Determine if the argument index is valid
        /// </summary>
        private static bool ValidIndex(int index)
        {
            return (index >= 0) && (index < Args.Count);
        }

        /// <summary>
        /// Remove named argument/value in cmd-line arguments
        /// </summary>
        private static void RemoveFlag(string arg, bool noValue = false)
        {
            int index = IndexOfFlag(arg);

            for (int i = 0; i < (noValue ? 1 : 2); i++)
            {
                Args.RemoveAt(index);
            }
        }

        /// <summary>
        /// Get value of an argument in cmd-line arguments
        /// </summary>
        private static string ArgsValueAt(int index)
        {
            if (!ValidIndex(index))
            {
                Error.Handle(Except.NamedArgs, Args[index - 1], true);
            }
            return Args[index];
        }

        /// <summary>
        /// Get executable path for command execution
        /// </summary>
        private static string GetExecutable(int argIndex)
        {
            if (!ValidIndex(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }

            string exec = ArgsValueAt(argIndex + 1);
            (bool exists, string path) = Command.ExistsOnPath(exec);

            // Failed to locate executable
            if (!exists)
            {
                Error.Handle(Except.ExePath, exec, true);
            }

            Program.UsingExe = true;
            PipeVariant = PipeType.Process;

            return path;
        }

        /// <summary>
        /// Get file path to write to or read from
        /// </summary>
        private static string GetTransfer(int argIndex)
        {
            if (!ValidIndex(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }
            string path = Path.GetFullPath(ArgsValueAt(argIndex + 1));

            // Invalid file path
            if (!File.Exists(path) && !Directory.GetParent(path).Exists)
            {
                Error.Handle(Except.FilePath, path, true);
            }

            PipeVariant = PipeType.File;
            return path;
        }

        /// <summary>
        /// Get file path to write to or read from
        /// </summary>
        private static string GetText(int argIndex)
        {
            if (!ValidIndex(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }
            string data = ArgsValueAt(argIndex + 1);

            // Invalid payload string
            if (data.Trim() is null or "")
            {
                Error.Handle(Except.Payload, Args[argIndex], true);
            }

            PipeVariant = PipeType.Text;
            return data;
        }

        /// <summary>
        /// Get port number from argument index
        /// </summary>
        private int GetPort(int argIndex)
        {
            if (!ValidIndex(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }
            string sPort = ArgsValueAt(argIndex + 1);

            // Handle invalid port strings
            if (!int.TryParse(sPort, out int port) || port is 0 or > 65535)
            {
                Console.WriteLine(GetUsage(_appTitle));
                Error.Handle(Except.InvalidPort, sPort);
            }
            return port;
        }
    }
}
