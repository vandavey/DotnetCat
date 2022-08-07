using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DotnetCat.Errors;
using DotnetCat.IO.FileSystem;
using DotnetCat.Network;
using DotnetCat.Pipelines;

namespace DotnetCat.Utils
{
    /// <summary>
    ///  Command-line argument parser and validator
    /// </summary>
    internal class Parser
    {
        private readonly string _eol;   // Platform EOL string

        private readonly string _help;  // Help information

        /// <summary>
        ///  Initialize object
        /// </summary>
        public Parser()
        {
            _eol = Environment.NewLine;
            _help = GetHelp();

            Args = new CmdLineArgs();
            ArgsList = new List<string>();
        }

        /// Application title string
        public static string AppTitle
        {
            get => Program.OS is Platform.Nix ? "dncat" : "dncat.exe";
        }

        /// Application repository URL
        public static string Repo => "https://github.com/vandavey/DotnetCat";

        /// Application usage string
        public static string Usage => $"Usage: {AppTitle} [OPTIONS] TARGET";

        /// Command-line arguments
        public CmdLineArgs Args { get; set; }

        /// Command-line argument list
        public List<string> ArgsList { get; set; }

        /// <summary>
        ///  Check for help flag in cmd-line arguments
        /// </summary>
        public static bool NeedsHelp(string[] args)
        {
            // Count matching arguments
            int count = (from arg in args.ToList()
                         where arg.ToLower() == "--help"
                             || (arg.Length > 1
                                 && arg[0] == '-'
                                 && arg[1] != '-'
                                 && (arg.Contains('h') || arg.Contains('?')))
                         select arg).Count();

            return count > 0;
        }

        /// <summary>
        ///  Get the matching cmd-line argument index
        /// </summary>
        public static int IndexOfFlag(List<string>? args,
                                      string flag,
                                      char? alias = default) {
            if (flag.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(flag));
            }

            if (flag == "-")
            {
                return args?.IndexOf(flag) ?? -1;
            }

            // Assign argument alias
            alias ??= flag.Where(c => char.IsLetter(c)).FirstOrDefault();

            // Query cmd-line arguments
            List<int> query = (from arg in args
                               where arg.ToLower() == flag.ToLower()
                                   || (arg.Contains(alias ?? '\0')
                                       && arg[0] == '-'
                                       && arg[1] != '-')
                               select args?.IndexOf(flag) ?? -1).ToList();

            return query.Count > 0 ? query[0] : -1;
        }

        /// <summary>
        ///  Parse the command-line arguments from the given array
        /// </summary>
        public CmdLineArgs Parse(string[] args)
        {
            ArgsList = DefragArguments(args);

            ParseCharArgs();
            ParseFlagArgs();
            ParsePositionalArgs();

            return Args;
        }

        /// <summary>
        ///  Get index of cmd-line argument with the specified char
        /// </summary>
        public int IndexOfAlias(char alias)
        {
            List<int> query = (from arg in ArgsList.ToList()
                               where arg.Contains(alias)
                                   && arg[0] == '-'
                                   && arg[1] != '-'
                               select ArgsList.IndexOf(arg)).ToList();

            return query.Count > 0 ? query[0] : -1;
        }

        /// <summary>
        ///  Get the matching cmd-line argument index
        /// </summary>
        public int IndexOfFlag(string flag, char? alias = default)
        {
            return IndexOfFlag(ArgsList, flag, alias);
        }

        /// <summary>
        ///  Print application help message to console output
        /// </summary>
        public void PrintHelp()
        {
            Console.WriteLine(_help);
            Environment.Exit(0);
        }

        /// <summary>
        ///  Defragment the given command-line arguments
        /// </summary>
        private static List<string> DefragArguments(string[] args)
        {
            int delta = 0;
            List<string> list = args.ToList();

            // Get arguments starting with quote
            var query = from arg in args
                        let pos = Array.IndexOf(args, arg)
                        let quote = arg.FirstOrDefault()
                        let valid = arg.EndsWith(quote) && arg.Length >= 2
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
                    string arg = string.Join(", ", args[item.pos..]);
                    Error.Handle(Except.StringEOL, arg, true);
                }
                else  // Calculate position delta
                {
                    delta = eolQuery.pos - item.pos;
                }

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
        ///  Parse named command-line arguments that start with one dash (e.g., -f)
        /// </summary>
        private void ParseCharArgs()
        {
            // Locate all char flag arguments
            var query = from arg in ArgsList.ToList()
                        let index = IndexOfFlag(arg)
                        where arg.Length >= 2
                            && arg[0] == '-'
                            && arg[1] != '-'
                        select new { arg, index };

            foreach (var item in query)
            {
                if (item.arg.Contains('l'))  // Listen for connection
                {
                    Args.Listen = true;
                    RemoveAlias(item.index, 'l');
                }

                if (item.arg.Contains('v'))  // Verbose output
                {
                    Args.Verbose = true;
                    RemoveAlias(item.index, 'v');
                }

                if (item.arg.Contains('z'))  // Zero-IO (test connection)
                {
                    Args.PipeVariant = PipeType.Status;
                    RemoveAlias(item.index, 'z');
                }

                if (item.arg.Contains('d'))  // Debug output
                {
                    Args.Debug = Args.Verbose = true;
                    RemoveAlias(item.index, 'd');
                }

                if (item.arg.Contains('p'))  // Connection port
                {
                    Args.Port = GetPort(item.index);
                    RemoveAlias(item.index, 'p', remValue: true);
                }

                if (item.arg.Contains('e'))  // Executable path
                {
                    Args.ExePath = GetExecutable(item.index);
                    RemoveAlias(item.index, 'e', remValue: true);
                }

                if (item.arg.Contains('o'))  // Receive file data
                {
                    Args.FilePath = GetTransfer(item.index);
                    Args.TransOpt = TransferOpt.Collect;
                    RemoveAlias(item.index, 'o', remValue: true);
                }

                if (item.arg.Contains('s'))  // Send file data
                {
                    Args.FilePath = GetTransfer(item.index);
                    Args.TransOpt = TransferOpt.Transmit;
                    RemoveAlias(item.index, 's', remValue: true);
                }

                if (item.arg.Contains('t'))  // Send string data
                {
                    Args.Payload = GetTextPayload(item.index);
                    RemoveAlias(item.index, 't', remValue: true);
                }

                if (ArgsValueAt(item.index) == "-")
                {
                    ArgsList.RemoveAt(IndexOfFlag("-"));
                }
            }
        }

        /// <summary>
        ///  Parse named command-line arguments that start with
        ///  two dashes (e.g., --foo)
        /// </summary>
        private void ParseFlagArgs()
        {
            // Locate all flag arguments
            var query = from arg in ArgsList.ToList()
                        let index = IndexOfFlag(arg)
                        where arg.StartsWith("--")
                        select new { arg, index };

            foreach (var item in query)
            {
                switch (item.arg)
                {
                    case "--listen":   // Listen for connection
                    {
                        Args.Listen = true;
                        ArgsList.RemoveAt(item.index);
                        break;
                    }
                    case "--verbose":  // Verbose output
                    {
                        Args.Verbose = true;
                        ArgsList.RemoveAt(item.index);
                        break;
                    }
                    case "--debug":    // Debug output
                    {
                        Args.Debug = Args.Verbose = true;
                        ArgsList.RemoveAt(item.index);
                        break;
                    }
                    case "--zero-io":  // Zero-IO (test connection)
                    {
                        Args.PipeVariant = PipeType.Status;
                        ArgsList.RemoveAt(item.index);
                        break;
                    }
                    case "--port":     // Connection port
                    {
                        Args.Port = GetPort(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--exec":     // Executable path
                    {
                        Args.ExePath = GetExecutable(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--text":     // Send string data
                    {
                        Args.Payload = GetTextPayload(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--output":   // Receive file data
                    {
                        Args.FilePath = GetTransfer(item.index);
                        Args.TransOpt = TransferOpt.Collect;
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--send":     // Send file data
                    {
                        Args.FilePath = GetTransfer(item.index);
                        Args.TransOpt = TransferOpt.Transmit;
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
        ///  Parse positional command-line arguments
        /// </summary>
        private void ParsePositionalArgs()
        {
            // Validate remaining cmd-line arguments
            switch (ArgsList.Count)
            {
                case 0:   // Missing TARGET
                {
                    if (!Args.Listen)
                    {
                        Error.Handle(Except.RequiredArgs, "TARGET", true);
                    }
                    break;
                }
                case 1:   // Validate TARGET
                {
                    if (ArgsList[0].StartsWith('-'))
                    {
                        Error.Handle(Except.UnknownArgs, ArgsList[0], true);
                    }
                    Exception? ex = default;

                    // Parse the connection IPv4 address
                    if (IPAddress.TryParse(ArgsList[0], out IPAddress? addr))
                    {
                        Args.Address = addr;
                    }
                    else  // Resolve the hostname
                    {
                        (Args.Address, ex) = Net.ResolveName(ArgsList[0]);
                    }

                    Args.HostName = ArgsList[0];

                    // Invalid destination host
                    if (Args.Address == IPAddress.Any)
                    {
                        Error.Handle(Except.InvalidAddr, ArgsList[0], true, ex: ex);
                    }
                    break;
                }
                default:  // Unexpected arguments
                {
                    string argsStr = ArgsList.ToArray().Join(", ");

                    if (ArgsList[0].StartsWithValue('-'))
                    {
                        Error.Handle(Except.UnknownArgs, argsStr, true);
                    }
                    Error.Handle(Except.InvalidArgs, argsStr, true);
                    break;
                }
            }
        }

        /// <summary>
        ///  Get application help message as a string
        /// </summary>
        private string GetHelp()
        {
            return string.Join(_eol, new string[]
            {
                $"DotnetCat ({Repo})",
                $"{Usage}{_eol}",
                $"Remote command shell application{_eol}",
                "Positional Arguments:",
                $"  TARGET                    Remote or local IPv4 address{_eol}",
                "Optional Arguments:",
                "  -h/-?,   --help           Show this help message and exit",
                "  -v,      --verbose        Enable verbose console output",
                "  -d,      --debug          Output verbose error information",
                "  -l,      --listen         Listen for incoming connections",
                "  -z,      --zero-io        Report connection status only",
                "  -p PORT, --port PORT      Specify port to use for endpoint.",
                "                            (Default: 44444)",
                "  -e EXEC, --exec EXEC      Executable process file path",
                "  -o PATH, --output PATH    Receive file from remote host",
                "  -s PATH, --send PATH      Send local file or folder",
                $"  -t DATA, --text DATA      Send string to remote host{_eol}",
                "Usage Examples:",
                $"  {AppTitle} --listen --exec powershell.exe",
                $"  {AppTitle} -d -p 44444 localhost",
                $"  {AppTitle} -vo test.txt -p 2009 192.168.1.9 {_eol}",
            });
        }

        /// <summary>
        ///  Remove character (alias) from a cmd-line argument
        /// </summary>
        private void RemoveAlias(int index,
                                 char alias,
                                 bool remValue = false) {

            // Invalid index received
            if (!ValidIndex(index) || (remValue && !ValidIndex(index + 1)))
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            ArgsList[index] = ArgsList[index].Replace(alias.ToString(), "");

            // Remove arg value if requested
            if (remValue)
            {
                ArgsList.RemoveAt(index + 1);
            }
        }

        /// <summary>
        ///  Determine if the argument index is valid
        /// </summary>
        private bool ValidIndex(int index)
        {
            return index >= 0 && index < ArgsList.Count;
        }

        /// <summary>
        ///  Remove named argument/value in cmd-line arguments
        /// </summary>
        private void RemoveFlag(string arg, bool noValue = false)
        {
            int index = IndexOfFlag(arg);

            for (int i = 0; i < (noValue ? 1 : 2); i++)
            {
                ArgsList.RemoveAt(index);
            }
        }

        /// <summary>
        ///  Get value of an argument in cmd-line arguments
        /// </summary>
        private string ArgsValueAt(int index)
        {
            if (!ValidIndex(index))
            {
                Error.Handle(Except.NamedArgs, ArgsList[index - 1], true);
            }
            return ArgsList[index];
        }

        /// <summary>
        ///  Get port number from argument index
        /// </summary>
        private int GetPort(int index)
        {
            if (!ValidIndex(index + 1))
            {
                Error.Handle(Except.NamedArgs, ArgsList[index], true);
            }
            string portStr = ArgsValueAt(index + 1);

            // Handle invalid port strings
            if (!int.TryParse(portStr, out int port) || !Net.IsValidPort(port))
            {
                Console.WriteLine(Usage);
                Error.Handle(Except.InvalidPort, portStr);
            }
            return port;
        }

        /// <summary>
        ///  Get executable path for command execution
        /// </summary>
        private string? GetExecutable(int index)
        {
            if (!ValidIndex(index + 1))
            {
                Error.Handle(Except.NamedArgs, ArgsList[index], true);
            }

            string exec = ArgsValueAt(index + 1);
            (string? path, bool exists) = FileSys.ExistsOnPath(exec);

            // Failed to locate executable
            if (!exists)
            {
                Error.Handle(Except.ExePath, exec, true);
            }

            Args.UsingExe = true;
            Args.PipeVariant = PipeType.Process;

            return path;
        }

        /// <summary>
        ///  Get file path to write to or read from
        /// </summary>
        private string GetTransfer(int index)
        {
            if (!ValidIndex(index + 1))
            {
                Error.Handle(Except.NamedArgs, ArgsList[index], true);
            }
            int pathPos = index + 1;

            string path = FileSys.ResolvePath(ArgsValueAt(pathPos)) ?? string.Empty;
            string parentPath = Directory.GetParent(path)?.FullName ?? string.Empty;

            bool pathExists = FileSys.FileExists(path);
            bool parentExists = FileSys.DirectoryExists(parentPath);

            // Invalid file path received
            if (!pathExists && (Args.TransOpt is TransferOpt.Transmit))
            {
                Error.Handle(Except.FilePath, path, true);
            }
            else if (!parentExists && (Args.TransOpt is TransferOpt.Collect))
            {
                Error.Handle(Except.FilePath, parentPath, true);
            }
            Args.PipeVariant = PipeType.File;

            return path;
        }

        /// <summary>
        ///  Get string network payload
        /// </summary>
        private string GetTextPayload(int index)
        {
            if (!ValidIndex(index + 1))
            {
                Error.Handle(Except.NamedArgs, ArgsList[index], true);
            }
            string data = ArgsValueAt(index + 1);

            // Invalid payload string
            if (data.IsNullOrEmpty())
            {
                Error.Handle(Except.Payload, ArgsList[index], true);
            }

            Args.PipeVariant = PipeType.Text;
            return data;
        }
    }
}
