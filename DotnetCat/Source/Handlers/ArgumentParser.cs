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
    class ArgumentParser
    {
        private readonly string _appTitle;

        private readonly CommandHandler _cmd;

        private readonly ErrorHandler _error;

        /// Initialize new object
        public ArgumentParser()
        {
            _cmd = new CommandHandler();
            _error = new ErrorHandler();

            _appTitle = (OS is Platform.Nix) ? "dncat" : "dncat.exe";
            Help = GetHelp(_appTitle, GetUsage(_appTitle));
        }

        public string Help { get; }

        public List<string> Args
        {
            get => Program.Args;
            set => Program.Args = value;
        }

        public Node SockNode
        {
            get => Program.SockNode;
            set => Program.SockNode = value;
        }

        private Platform OS => Program.OS;

        private bool Debug { set => Program.Debug = value; }

        private bool Recursive { set => Program.Recursive = value; }

        public static string GetUsage(string appTitle = "dncat")
        {
            return $"Usage: {appTitle} [OPTIONS] TARGET";
        }

        /// Print application help message to console output
        public void PrintHelp()
        {
            Console.WriteLine(Help);
            Environment.Exit(0);
        }

        /// Parse named arguments starting with one dash
        public void ParseCharArgs()
        {
            // Locate all char flag arguments
            var query = from arg in Args.ToList()
                        let index = IndexOfFlag(arg)
                        where arg[0] == '-'
                            && arg[1] != '-'
                        select new { arg, index };

            foreach (var item in query)
            {
                if (item.arg.Contains('l')) // Listen for connection
                {
                    RemoveAlias(item.index, 'l');
                }

                if (item.arg.Contains('v')) // Verbose output
                {
                    SockNode.Verbose = true;
                    RemoveAlias(item.index, 'v');
                }

                if (item.arg.Contains('d')) // Debug output
                {
                    Debug = SockNode.Verbose = true;
                    RemoveAlias(item.index, 'd');
                }

                if (item.arg.Contains('r')) // Recursive transfer
                {
                    Recursive = true;
                    RemoveAlias(item.index, 'r');
                }

                if (item.arg.Contains('p')) // Connection port
                {
                    SockNode.Port = GetPort(item.index);
                    RemoveAlias(item.index, 'p');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('e')) // Executable path
                {
                    SockNode.Exe = GetExec(item.index);
                    RemoveAlias(item.index, 'e');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('o')) // Receive file data
                {
                    SockNode.FilePath = GetTransfer(item.index);
                    RemoveAlias(item.index, 'o');
                    Args.RemoveAt(item.index + 1);
                }

                if (item.arg.Contains('s')) // Send file data
                {
                    SockNode.FilePath = GetTransfer(item.index);
                    RemoveAlias(item.index, 's');
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
                    case "--listen": // Listen for connection
                    {
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--verbose": // Verbose output
                    {
                        SockNode.Verbose = true;
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--debug": // Debug output
                    {
                        Debug = SockNode.Verbose = true;
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--recurse": // Recursive transfer
                    {
                        Recursive = true;
                        Args.RemoveAt(item.index);
                        break;
                    }
                    case "--port": // Connection port
                    {
                        SockNode.Port = GetPort(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--exec": // Executable path
                    {
                        SockNode.Exe = GetExec(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--output": // Receive file data
                    {
                        SockNode.FilePath = GetTransfer(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
                        break;
                    }
                    case "--send": // Send file data
                    {
                        SockNode.FilePath = GetTransfer(item.index);
                        RemoveFlag(ArgsValueAt(item.index));
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

        /// Get value of an argument in cmd-line arguments
        public string ArgsValueAt(int index)
        {
            if ((index < 0) || (index >= Args.Count))
            {
                _error.Handle(Except.NamedArgs, Args[index - 1], true);
            }
            return Args[index];
        }

        /// Check for help flag in cmd-line arguments
        public bool NeedsHelp(string[] args)
        {
            // Query cmd-line arguments
            List<string> query = (from arg in args
                                  where arg.ToLower() == "--help"
                                      || (arg.Length > 1
                                          && arg[0] == '-'
                                          && arg[1] != '-'
                                          && (arg.Contains('h')
                                              || arg.Contains('?')))
                                  select arg).ToList();

            return query.Count() > 0;
        }

        /// Remove named argument/value in cmd-line arguments
        public void RemoveFlag(string arg, bool noValue = false)
        {
            int index = IndexOfFlag(arg);
            int length = noValue ? 1 : 2;

            for (int i = 0; i < length; i++)
            {
                Args.RemoveAt(index);
            }
        }

        /// Remove character of a cmd-line argument
        public void RemoveAlias(int index, char alias)
        {
            if (index < 0 || (index >= Args.Count()))
            {
                throw new IndexOutOfRangeException($"{nameof(index)}");
            }
            Args[index] = Args[index].Replace(alias.ToString(), "");
        }

        /// Get executable path for command execution
        public string GetExec(int argIndex)
        {
            string exec = ArgsValueAt(argIndex + 1);
            (bool exists, string path) = _cmd.ExistsOnPath(exec);

            // Failed to locate executable
            if (!exists)
            {
                _error.Handle(Except.ExecPath, exec, true);
            }

            Program.UsingExe = true;
            return path;
        }

        /// Get file path to write to or read from
        public string GetTransfer(int argIndex)
        {
            string path = Path.GetFullPath(ArgsValueAt(argIndex + 1));

            // Invalid file path
            if (!File.Exists(path) && !Directory.GetParent(path).Exists)
            {
                _error.Handle(Except.FilePath, path);
            }
            return path;
        }

        /// Get port number from argument index
        public int GetPort(int argIndex)
        {
            int iPort = -1;
            string port = ArgsValueAt(argIndex + 1);

            try // Validate port
            {
                if (((iPort = int.Parse(port)) < 0) || (iPort > 65535))
                {
                    throw new FormatException();
                }
            }
            catch (FormatException ex) // Invalid port number
            {
                _error.Handle(Except.InvalidPort, port, ex);
            }
            return iPort;
        }

        /// Get application help message as a string
        private static string GetHelp(string appTitle, string appUsage)
        {
            return string.Join("\r\n", new string[]
            {
                "DotnetCat (https://github.com/vandavey/DotnetCat)",
                $"{appUsage}\r\n",
                "Remote command shell application\r\n",
                "Positional Arguments:",
                "  TARGET                   Specify remote/local IPv4 address\r\n",
                "Optional Arguments:",
                "  -h/-?,   --help           Show this help message and exit",
                "  -v,      --verbose        Enable verbose console output",
                "  -d,      --debug          Output verbose error information",
                "  -l,      --listen         Listen for incoming connections",
                "  -r,      --recurse        Transfer a directory recursively",
                "  -p PORT, --port PORT      Specify port to use for endpoint.",
                "                            (Default: 4444)",
                "  -e EXEC, --exec EXEC      Specify command shell executable",
                "  -o PATH, --output PATH    Receive file from remote host",
                "  -s PATH, --send PATH      Send local file or folder\r\n",
                "Usage Examples:",
                $"  {appTitle} -le powershell.exe",
                $"  {appTitle} 10.0.0.152 -p 4444 localhost",
                $"  {appTitle} -ve /bin/bash 192.168.1.9\r\n",
            });
        }
    }
}
