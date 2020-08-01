using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Env = System.Environment;

namespace DotnetCat
{
    /// <summary>
    /// Execute special commands on the local system
    /// </summary>
    class CommandHandler : ICloseable
    {
        private readonly List<string> _commands;
        private readonly List<string> _envPath;
        private readonly List<string> _extensions;

        private StreamReader _reader;

        /// Initialize new CommandHandler
        public CommandHandler()
        {
            string path = Env.GetEnvironmentVariable("PATH");
            _envPath = path.Split(Path.PathSeparator).ToList();

            _extensions = new List<string>
            {
                "exe", "ps1", "py", "sh", "bat"
            };

            _commands = new List<string>
            {
                "about", "download", "upload"
            };
        }

        /// Determine if specified command is DotNetCat keyword
        public bool IsSpecialCommand(string cmd)
        {
            if (_commands.Contains(cmd.ToLower()))
            {
                return true;
            }

            return false;
        }

        /// Get default command shell for the platform
        public string DefaultShell()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!ExistsOnPath("/bin/bash").exists)
                {
                    return "/bin/sh";
                }

                return "/bin/bash";
            }

            if (!ExistsOnPath("powershell.exe").exists)
            {
                return "cmd.exe";
            }

            return "powershell.exe";
        }

        /// Determine if executable exists on environment path
        public (bool exists, string) ExistsOnPath(string shell)
        {
            if (File.Exists(shell))
            {
                return (true, Path.GetFullPath(shell));
            }

            string path;

            if ((path = GetShellPath(shell)) != null)
            {
                return (true, path);
            }

            if (!Path.HasExtension(shell))
            {
                foreach (string ext in _extensions)
                {
                    string name = Path.ChangeExtension(shell, ext);

                    if ((path = GetShellPath(name)) != null)
                    {
                        return (true, path);
                    }
                }
            }

            return (false, null);
        }

        /// Search environment path for specified shell
        public string GetShellPath(string shell)
        {
            foreach (string path in _envPath)
            {
                string testPath = Path.Combine(path, shell);

                if (File.Exists(testPath))
                {
                    return testPath;
                }
            }

            return null;
        }

        /// Get file path to the current user's profile
        public string GetProfilePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Env.GetEnvironmentVariable("USERPROFILE");
            }

            return Env.GetEnvironmentVariable("HOME");
        }

        public void Close()
        {
            _reader?.Close();
        }
    }
}
