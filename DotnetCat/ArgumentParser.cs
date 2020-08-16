using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using DotnetCat.Handlers;
using DotnetCat.Nodes;
using DotnetCat.Utils;

namespace DotnetCat
{
    /// <summary>
    /// Command line argument parser and validator
    /// </summary>
    class ArgumentParser
    {
        private readonly CommandHandler _cmd;

        private readonly ErrorHandler _error;

        /// Initialize new ArgumentParser
        public ArgumentParser()
        {
            _cmd = new CommandHandler();
            _error = new ErrorHandler();
            string appTitle = GetAppTitle();

            this.Usage = $"Usage: {appTitle} [OPTIONS] TARGET";
            this.Help = GetHelp(appTitle, this.Usage);
        }

        public string Help { get; }

        public string Usage { get; }

        public List<string> Args
        {
            get => Program.Args;
            set => Program.Args = value;
        }

        public static string GetUsage()
        {
            return $"Usage: {GetAppTitle()} [OPTIONS] TARGET";
        }

        /// Print application help message to console output
        public void PrintHelp()
        {
            Console.WriteLine(Help);
            Environment.Exit(0);
        }

        /// Get the index of an argument in cmd-line arguments
        public int IndexOfArgs(string name, string flag = null)
        {
            flag ??= name;
            int argIndex = -1;

            List<int> query = (from arg in Args
                               where arg.ToLower() == flag.ToLower()
                                   || arg.ToLower() == name.ToLower()
                               select Args.IndexOf(arg)).ToList();

            query.ForEach(index => argIndex = index);
            return argIndex;
        }

        /// Get index of an argument containing specified character
        public int IndexOfFlag(char flag)
        {
            int flagIndex = -1;

            List<int> query = (from arg in Args
                               where arg.StartsWith("-")
                                   && !arg.StartsWith("--")
                                   && arg.Contains(flag)
                               select Args.IndexOf(arg)).ToList();

            query.ForEach(index => flagIndex = index);
            return flagIndex;
        }

        /// Get value of an argument in cmd-line arguments
        public string ArgsValueAt(int index)
        {
            if ((index < 0) || (index >= Args.Count))
            {
                _error.Handle(ErrorType.NamedArg, Args[index - 1], true);
            }

            return Args[index];
        }

        /// Check for help flag in cmd-line arguments
        public bool NeedsHelp(string[] args)
        {
            int argIndex = -1;

            List<int> query = (from arg in args
                               where arg.ToLower() == "--help"
                                   || (arg[0] == '-'
                                       && arg[1] != '-'
                                       && (arg.Contains('h')
                                           || arg.Contains('?')))
                               select Array.IndexOf(args, arg)).ToList();

            query.ForEach(index => argIndex = index);
            return argIndex > -1;
        }

        /// Remove named argument/value in cmd-line arguments
        public void RemoveNamedArg(string arg)
        {
            arg = arg.StartsWith("--") ? arg : $"--{arg}";
            int index = IndexOfArgs(arg);

            Args.RemoveAt(index);
            Args.RemoveAt(index++);
        }

        /// Update character of a cmd-line argument
        public void UpdateArgs(int index, char flag)
        {
            Args[index] = Args[index].Replace($"{flag}", "");
        }

        /// Determine if specified address is a valid IPV4 address
        public bool AddressIsValid(string addr)
        {
            try
            {
                IPAddress.Parse(addr);
                return true;
            }
            catch (Exception ex)
            {
                if (!(ex is FormatException))
                {
                    throw ex;
                }
                return false;
            }
        }

        /// Specify local/remote IPv4 address to use
        public SocketShell SetAddress(SocketShell shell, string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                throw new ArgumentNullException("addr");
            }

            if (!AddressIsValid(addr))
            {
                _error.Handle(ErrorType.InvalidAddress, addr, true);
            }

            shell.Address = IPAddress.Parse(addr);
            return shell;
        }

        /// Specify shell executable for command execution
        public SocketShell SetExec(SocketShell shell, string exec)
        {
            if (string.IsNullOrEmpty(exec))
            {
                throw new ArgumentNullException("shell");
            }

            (bool exists, string path) = _cmd.ExistsOnPath(exec);

            if (!exists)
            {
                _error.Handle(ErrorType.ShellPath, exec, true);
            }

            shell.Executable = path;
            return shell;
        }

        /// Specify file path for file stream manipulation
        public SocketShell SetFilePath(SocketShell shell, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                _error.Handle(ErrorType.FilePath, path);
            }

            shell.ShellPath = path;
            return shell;
        }

        /// Specify the port to use for connection
        public SocketShell SetPort(SocketShell shell, string port)
        {
            if (string.IsNullOrEmpty(port))
            {
                throw new ArgumentNullException("portString");
            }

            int portNum = -1;

            try
            {
                portNum = int.Parse(port);
            }
            catch (Exception ex)
            {
                if (!(ex is FormatException))
                {
                    throw ex;
                }
                _error.Handle(ErrorType.InvalidPort, port);
            }

            if ((portNum < 0) || (portNum > 65535))
            {
                _error.Handle(ErrorType.InvalidPort, port);
            }

            if (shell is SocketServer)
            {
                shell.Port = portNum;
            }
            else
            {
                shell.Port = int.Parse(port);
            }

            return shell;
        }

        /// Enable verbose program console output
        public SocketShell SetVerbose(SocketShell shell)
        {
            shell.IsVerbose = true;
            return shell;
        }

        /// Get program title based on platform
        private static string GetAppTitle()
        {
            if (Program.GetPlatform() == Platform.Windows)
            {
                return "dncat.exe";
            }

            return "dncat";
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
                "  -h/-?,   --help          Show this help message and exit",
                "  -v,      --verbose       Enable verbose console output",
                "  -l,      --listen        Listen for incoming connections",
                "  -p PORT, --port PORT     Specify port to use for socket.",
                "                           (Default: 4444)",
                "  -e EXEC, --exec EXEC     Specify command shell executable",
                "  -r PATH, --recv PATH     Receive remote file or folder",
                "  -s PATH, --send PATH     Send local file or folder\r\n",
                "Usage Examples:",
                $"  {appTitle} 10.0.0.152",
                $"  {appTitle} -le powershell.exe -p 4444 127.0.0.1",
                $"  {appTitle} -ve /bin/bash 192.168.1.9\r\n",
            });
        }
    }
}
