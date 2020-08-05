using DotnetCat.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Prog = DotnetCat.Program;

namespace DotnetCat
{
    /// <summary>
    /// Command line argument parser and validator
    /// </summary>
    class ArgumentParser
    {
        private readonly CommandHandler _cmd;

        /// Initialize new ArgumentParser
        public ArgumentParser()
        {
            _cmd = new CommandHandler();
            string appTitle = GetAppTitle(_cmd);

            this.UsageText = $"Usage: {appTitle} [OPTIONS] TARGET";
            this.HelpText = GetHelp(appTitle, this.UsageText);
        }

        public string UsageText { get; }

        public string HelpText { get; }

        /// Print application help message to console output
        public void PrintHelp()
        {
            Console.WriteLine(HelpText);
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
                Prog.Error.Handle("flag", Prog.Args[index - 1], true);
            }

            return Prog.Args[index];
        }

        /// Check for help flag in command line arguments
        public bool NeedsHelp(string[] args)
        {
            int index = -1;
            
            List<int> query = (from arg in args
                               let chars = arg.ToLower().ToList()
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
        public void SetAddress(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                throw new ArgumentNullException("addr");
            }

            if (!AddressIsValid(addr))
            {
                Prog.Error.Handle("address", addr, true);
            }

            if (Prog.Node == "server")
            {
                Prog.Server.Address = IPAddress.Parse(addr);
            }
            else
            {
                Prog.Client.Address = IPAddress.Parse(addr);
            }
        }

        /// Specify shell executable to use for command execution
        public void SetExec(string shell)
        {
            if (string.IsNullOrEmpty(shell))
            {
                throw new ArgumentNullException("shell");
            }

            (bool exists, string path) = _cmd.ExistsOnPath(shell);

            if (!exists)
            {
                Prog.Error.Handle("shell", shell, usage: true);
            }

            if (Prog.Node == "server")
            {
                Prog.Server.Shell = path;
            }
            else
            {
                Prog.Client.Shell = path;
            }
        }

        /// Specify file path to use for file stream
        public void SetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Prog.Error.Handle("path", path);
            }

            if (Prog.Node == "server")
            {
                Prog.Server.FilePath = path;
            }
            else
            {
                Prog.Client.FilePath = path;
            }
        }

        /// Specify string value of port to use for connection
        public void SetPort(string portString)
        {
            int port = -1;

            if (string.IsNullOrEmpty(portString))
            {
                throw new ArgumentNullException("portString");
            }

            try
            {
                port = int.Parse(portString);
            }
            catch (Exception ex)
            {
                if (!(ex is FormatException))
                {
                    throw ex;
                }

                Prog.Error.Handle("port", portString);
            }

            if ((port < 0) || (port > 65535))
            {
                Prog.Error.Handle("port", portString);
            }

            if (Prog.Node == "server")
            {
                Prog.Server.Port = port;
            }
            else
            {
                Prog.Client.Port = int.Parse(portString);
            }
        }

        /// Enable verbose standard console output
        public void SetVerbose()
        {
            if (Prog.Node == "server")
            {
                Prog.Server.Verbose = true;
            }
            else
            {
                Prog.Client.Verbose = true;
            }
        }

        /// Get program title based on platform
        private static string GetAppTitle(CommandHandler cmd)
        {
            return cmd.IsWindowsPlatform ? "dncat.exe" : "dncat";
        }

        /// Get application help message as a string
        private static string GetHelp(string appTitle, string usage)
        {
            return string.Join("\r\n", new string[]
            {
                "DotnetCat (https://github.com/vandavey/DotnetCat)",
                $"{usage}\r\n",
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
