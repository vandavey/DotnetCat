using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCat.Enums;
using DotnetCat.Nodes;
using Cmd = DotnetCat.Handlers.CommandHandler;
using Error = DotnetCat.Handlers.ErrorHandler;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Command line argument parser and validator
    /// </summary>
    class ArgumentParser
    {
        private readonly string _appTitle;
        private readonly string _help;

        /// Initialize object
        public ArgumentParser()
        {
            _appTitle = (OS is Platform.Nix) ? "dncat" : "dncat.exe";
            _help = GetHelp(_appTitle, GetUsage(_appTitle));
        }

        private Platform OS => Program.OS;

        private bool Debug { set => Program.Debug = value; }

        private PipeType PipeVariant { set => Program.PipeVariant = value; }

        private string Payload
        {
            set => Program.Payload = value;
        }

        private List<string> Args
        {
            get => Program.Args;
            set => Program.Args = value;
        }

        private Node SockNode
        {
            get => Program.SockNode;
            set => Program.SockNode = value;
        }

        public static string GetUsage(string appTitle = "dncat")
        {
            return $"Usage: {appTitle} [OPTIONS] TARGET";
        }

        /// Check for help flag in cmd-line arguments
        public bool NeedsHelp(string[] args)
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

        /// Print application help message to console output
        public void PrintHelp()
        {
            Console.WriteLine(_help);
            Environment.Exit(0);
        }

        /// Parse named arguments starting with one dash
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
                    RemoveAlias(item.index, 'p');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('e'))  // Executable path
                {
                    SockNode.Exe = GetExecutable(item.index);
                    RemoveAlias(item.index, 'e');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('o'))  // Receive file data
                {
                    SockNode.FilePath = GetTransfer(item.index);
                    RemoveAlias(item.index, 'o');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('s'))  // Send file data
                {
                    SockNode.FilePath = GetTransfer(item.index);
                    RemoveAlias(item.index, 's');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('t'))  // Send string data
                {
                    Payload = GetText(item.index);
                    RemoveAlias(item.index, 't');
                    Args.RemoveAt(item.index + 1);
                }

                if (ArgsValueAt(item.index) == "-")
                {
                    Args.RemoveAt(IndexOfFlag("-"));
                }
            }
        }

        /// Parse named arguments starting with two dashes
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
                    case "--output":   // Receive file data
                    {
                        SockNode.FilePath = GetTransfer(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--send":     // Send file data
                    {
                        SockNode.FilePath = GetTransfer(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--text":     // Send string data
                    {
                        Payload = GetText(item.index);
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

        /// Get index of cmd-line argument with the specified char
        public int IndexOfAlias(char alias)
        {
            // Query cmd-line arguments
            List<int> query = (from arg in Args
                               where arg.Contains(alias)
                                   && arg[0] == '-'
                                   && arg[1] != '-'
                               select Args.IndexOf(arg)).ToList();

            return (query.Count() > 0) ? query[0] : -1;
        }

        /// Get the index of argument in cmd-line arguments
        public int IndexOfFlag(string flag, char? alias = null)
        {
            if (flag == "-")
            {
                return Args.IndexOf(flag);
            }

            // Assign argument alias
            if (string.IsNullOrEmpty(alias.ToString()))
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

            return (query.Count() > 0) ? query[0] : -1;
        }

        /// Get application help message as a string
        private static string GetHelp(string appTitle, string appUsage)
        {
            string lf = Environment.NewLine;

            return string.Join(lf, new string[]
            {
                "DotnetCat (https://github.com/vandavey/DotnetCat)",
                $"{appUsage}{lf}",
                $"Remote command shell application{lf}",
                "Positional Arguments:",
                $"  TARGET                   Remote/local IPv4 address{lf}",
                "Optional Arguments:",
                "  -h/-?,   --help           Show this help message and exit",
                "  -v,      --verbose        Enable verbose console output",
                "  -d,      --debug          Output verbose error information",
                "  -l,      --listen         Listen for incoming connections",
                "  -t,      --text           Send string data to remote host",
                "  -p PORT, --port PORT      Specify port to use for endpoint.",
                "                            (Default: 4444)",
                "  -e EXEC, --exec EXEC      Executable process file path",
                "  -o PATH, --output PATH    Receive file from remote host",
                $"  -s PATH, --send PATH      Send local file or folder{lf}",
                "Usage Examples:",
                $"  {appTitle} -le powershell.exe",
                $"  {appTitle} 10.0.0.152 -p 4444 localhost",
                $"  {appTitle} -ve /bin/bash 192.168.1.9{lf}",
            });
        }

        /// Get value of an argument in cmd-line arguments
        private string ArgsValueAt(int index)
        {
            if (!InArgsRange(index))
            {
                Error.Handle(Except.NamedArgs, Args[index - 1], true);
            }
            return Args[index];
        }

        /// Determine if the argument index is valid
        private bool InArgsRange(int index)
        {
            return (index >= 0) && (index < Args.Count());
        }

        /// Remove character (alias) from a cmd-line argument
        private void RemoveAlias(int index, char alias)
        {
            if (!InArgsRange(index))
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            Args[index] = Args[index].Replace(alias.ToString(), "");
        }

        /// Remove named argument/value in cmd-line arguments
        private void RemoveFlag(string arg, bool noValue = false)
        {
            int index = IndexOfFlag(arg);

            for (int i = 0; i < (noValue ? 1 : 2); i++)
            {
                Args.RemoveAt(index);
            }
        }

        /// Get port number from argument index
        private int GetPort(int argIndex)
        {
            if (!InArgsRange(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }

            int iPort = -1;
            string port = ArgsValueAt(argIndex + 1);

            try  // Validate port
            {
                if (((iPort = int.Parse(port)) < 0) || (iPort > 65535))
                {
                    throw new FormatException();
                }
            }
            catch (FormatException ex)  // Invalid port number
            {
                Console.WriteLine(GetUsage(_appTitle));
                Error.Handle(Except.InvalidPort, port, ex);
            }
            return iPort;
        }

        /// Get executable path for command execution
        private string GetExecutable(int argIndex)
        {
            if (!InArgsRange(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }

            string exec = ArgsValueAt(argIndex + 1);
            (bool exists, string path) = Cmd.ExistsOnPath(exec);

            // Failed to locate executable
            if (!exists)
            {
                Error.Handle(Except.ExePath, exec, true);
            }

            Program.UsingExe = true;
            PipeVariant = PipeType.Process;

            return path;
        }

        /// Get file path to write to or read from
        private string GetTransfer(int argIndex)
        {
            if (!InArgsRange(argIndex + 1))
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

        /// Get file path to write to or read from
        private string GetText(int argIndex)
        {
            if (!InArgsRange(argIndex + 1))
            {
                Error.Handle(Except.NamedArgs, Args[argIndex], true);
            }
            string data = ArgsValueAt(argIndex + 1);

            // Invalid payload string
            if (string.IsNullOrEmpty(data))
            {
                Error.Handle(Except.Payload, Args[argIndex], true);
            }

            PipeVariant = PipeType.Text;
            return data;
        }
    }
}
