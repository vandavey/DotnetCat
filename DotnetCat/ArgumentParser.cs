using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using DotnetCat.Handlers;
using DotnetCat.Nodes;
using Prog = DotnetCat.Program;

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

        /// Get the local system's platform
        public Platform SysPlatform
        {
            get
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Platform.Linux;
                }

                return Platform.Windows;
            }
        }

        public static string GetUsage()
        {
            string appTitle = GetAppTitle();
            return $"Usage: {appTitle} [OPTIONS] TARGET";
        }

        /// Print application help message to console output
        public void PrintHelp()
        {
            Console.WriteLine(Help);
            Environment.Exit(0);
        }

        /// Get the index of an argument in Program.Args
        public int IndexOfArgs(string name, string abrev = null)
        {
            int index = -1;
            abrev = string.IsNullOrEmpty(abrev) ? name : abrev;

            List<int> query = (from arg in Prog.Args
                               where arg.ToLower() == abrev.ToLower()
                                   || arg.ToLower() == name.ToLower()
                               select Prog.Args.IndexOf(arg)).ToList();

            query.ForEach(x => index = x);
            return index;
        }

        /// Get index of an argument containing specified character
        public int IndexOfFlag(char letter)
        {
            int index = -1;

            List<int> query = (from arg in Prog.Args
                               where arg.StartsWith("-")
                                   && !arg.StartsWith("--")
                                   && arg.Contains(letter)
                               select Prog.Args.IndexOf(arg)).ToList();

            query.ForEach(x => index = x);
            return index;
        }

        /// Get value of an argument in Program.Args
        public string ArgsValueAt(int index)
        {
            if ((index < 0) || (index >= Prog.Args.Count))
            {
                _error.Handle("flag", Prog.Args[index - 1], true);
            }

            return Prog.Args[index];
        }

        /// Check for help flag in command line arguments
        public bool NeedsHelp(string[] args)
        {
            int index = -1;

            List<int> query = (from arg in args
                               let chars = arg.ToList()
                               where arg.ToLower() == "-h"
                                   || arg.ToLower() == "--help"
                                   || (arg.StartsWith('-')
                                       && (chars.Contains('h')
                                           || chars.Contains('?')))
                               select args.ToList().IndexOf(arg)).ToList();

            query.ForEach(x => index = x);
            return index > -1;
        }

        /// Remove a named argument from Program.Args
        public void RemoveNamedArg(string flag)
        {
            flag = flag.StartsWith("--") ? flag : $"--{flag}";
            int index = IndexOfArgs(flag);

            Prog.Args.RemoveAt(index);
            Prog.Args.RemoveAt(index++);
        }

        /// Update a character of an argument in Program.Args
        public void UpdateArgs(int index, char character)
        {
            string ch = character.ToString();
            Prog.Args[index] = Prog.Args[index].Replace(ch, "");
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
                _error.Handle("address", addr, true);
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
                _error.Handle("shell", exec, true);
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
                _error.Handle("filepath", path);
            }

            shell.ShellPath = path;
            return shell;
        }

        /// Specify the port to use with socket
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
                _error.Handle("port", port);
            }

            if ((portNum < 0) || (portNum > 65535))
            {
                _error.Handle("port", port);
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

        /// Enable verbose standard console output
        public SocketShell SetVerbose(SocketShell shell)
        {
            shell.IsVerbose = true;
            return shell;
        }

        /// Get program title based on platform
        private static string GetAppTitle()
        {
            if (Prog.SysPlatform == Platform.Windows)
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
                "C# TCP socket command shell application\r\n",
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
                $"  {appTitle} -le /bin/bash",
                $"  {appTitle} -ve powershell.exe -p 5555 127.0.0.1",
                $"  {appTitle} -p 8152 127.0.0.1\r\n"
            });
        }
    }
}
