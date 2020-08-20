using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DotnetCat.Handlers;
using DotnetCat.Nodes;
using DotnetCat.Utils;

namespace DotnetCat
{
    enum ArgumentType { Flag, Named }

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

        public SocketShell SockShell
        {
            get => Program.SockShell;
            set => Program.SockShell = value;
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
                                   || (arg.Length > 1
                                       && arg[0] == '-'
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
            Args.RemoveAt(index + 1);
        }

        /// Update character of a cmd-line argument
        public void UpdateArgs(int index, char flag)
        {
            Args[index] = Args[index].Replace($"{flag}", "");
        }

        /// Enable verbose standard output
        public void SetVerbose(int argIndex, ArgumentType type)
        {
            SockShell.IsVerbose = true;

            if (type == ArgumentType.Named)
            {
                Args.RemoveAt(argIndex);
            }
            else
            {
                UpdateArgs(argIndex, 'v');
            }
        }

        /// Specify the local or remote host
        public void SetAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException("addr");
            }

            (bool isValid, IPAddress addr) = IsValidAddress(address);

            if (!isValid)
            {
                _error.Handle(ErrorType.InvalidAddress, address, true);
            }

            SockShell.Address = addr;
        }

        /// Determine if specified address is valid
        public (bool valid, IPAddress) IsValidAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return (false, null);
            }

            try
            {
                return (true, IPAddress.Parse(address));
            }
            catch (Exception ex)
            {
                if (!(ex is FormatException))
                {
                    throw ex;
                }
            }

            IPAddress addr = ResolveHostName(address);

            if (addr != null)
            {
                return (true, addr);
            }

            return (false, null);
        }

        /// Resolve the IPv4 address of given hostname
        public IPAddress ResolveHostName(string hostName)
        {
            IPHostEntry dnsAns;
            string machineName = Environment.MachineName.ToLower();

            try
            {
                dnsAns = Dns.GetHostEntry(hostName);
            }
            catch (SocketException)
            {
                return null;
            }

            if (dnsAns.HostName.ToLower() != machineName)
            {
                foreach (IPAddress addr in dnsAns.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return addr;
                    }
                }
                return null;
            }

            Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp
            );

            using (socket)
            {
                socket.Connect("8.8.8.8", 53);
                return (socket.LocalEndPoint as IPEndPoint).Address;
            }
        }

        /// Specify shell executable for command execution
        public void SetExec(int argIndex, ArgumentType type)
        {
            string exec = ArgsValueAt(argIndex + 1);
            (bool exists, string path) = _cmd.ExistsOnPath(exec);

            if (!exists)
            {
                _error.Handle(ErrorType.ShellPath, exec, true);
            }

            Program.IsUsingExec = true;
            SockShell.Executable = path;

            if (type == ArgumentType.Named)
            {
                RemoveNamedArg("exec");
            }
            else
            {
                UpdateArgs(argIndex, 'e');
                Args.RemoveAt(argIndex + 1);
            }
        }

        /// Set file tranfer type of socket shell to "recv"
        public void SetRecv(int argIndex, ArgumentType type)
        {
            string path = ArgsValueAt(argIndex + 1);
            SetFilePath(path);

            if (type == ArgumentType.Named)
            {
                RemoveNamedArg("recv");
                return;
            }

            UpdateArgs(argIndex, 'r');
            Args.RemoveAt(argIndex + 1);
        }

        /// Set file tranfer type of socket shell to "send"
        public void SetSend(int argIndex, ArgumentType type)
        {
            string path = ArgsValueAt(argIndex + 1);
            SetFilePath(path);

            if (type == ArgumentType.Named)
            {
                RemoveNamedArg("send");
                return;
            }

            UpdateArgs(argIndex, 's');
            Args.RemoveAt(argIndex + 1);
        }

        /// Specify file path for file stream manipulation
        public void SetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                _error.Handle(ErrorType.FilePath, path);
            }

            SockShell.ShellPath = path;
        }

        /// Specify the port to use for connection
        public void SetPort(int argIndex, ArgumentType type)
        {
            int portNum = -1;
            string port = ArgsValueAt(argIndex + 1);

            try
            {
                portNum = int.Parse(port);

                if ((portNum < 0) || (portNum > 65535))
                {
                    throw new FormatException();
                }
            }
            catch (Exception ex)
            {
                if (!(ex is FormatException))
                {
                    throw ex;
                }
                _error.Handle(ErrorType.InvalidPort, port);
            }

            SockShell.Port = portNum;

            if (type == ArgumentType.Named)
            {
                RemoveNamedArg("port");
                return;
            }

            UpdateArgs(argIndex, 'p');
            Args.RemoveAt(argIndex + 1);
        }

        /// Get program title based on platform
        private static string GetAppTitle()
        {
            if (Program.SysPlatform == Platform.Windows)
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
                $"  {appTitle} -le powershell.exe",
                $"  {appTitle} 10.0.0.152 -p 4444 localhost",
                $"  {appTitle} -ve /bin/bash 192.168.1.9\r\n",
            });
        }
    }
}
