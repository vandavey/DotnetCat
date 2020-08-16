using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Env = System.Environment;

namespace DotnetCat.Handlers
{
    enum Platform { Linux, Windows }

    /// <summary>
    /// Execute special commands on the local system
    /// </summary>
    class CommandHandler
    {
        private readonly List<string> _envPaths;

        private readonly List<string> _extensions;

        /// Initialize new CommandHandler
        public CommandHandler()
        {
            string path = Env.GetEnvironmentVariable("PATH");
            _envPaths = path.Split(Path.PathSeparator).ToList();

            _extensions = new List<string>
            {
                "exe", "bat", "ps1", "py", "sh"
            };
        }

        /// Get default command shell for the platform
        public string GetDefaultShell(Platform platform)
        {
            if (platform == Platform.Linux)
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

            string path = GetShellPath(shell);

            if (path != null)
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
            foreach (string envPath in _envPaths)
            {
                string path = Path.Combine(envPath, shell);

                if (File.Exists(Path.GetFullPath(path)))
                {
                    return path;
                }
            }

            return null;
        }

        /// Get file path to the current user's profile
        public string GetProfilePath(Platform platform)
        {
            if (platform == Platform.Windows)
            {
                return Env.GetEnvironmentVariable("USERPROFILE");
            }

            return Env.GetEnvironmentVariable("HOME");
        }
    }
}
