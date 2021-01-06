using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCat.Enums;
using ArgNullException = System.ArgumentNullException;
using Env = System.Environment;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Execute special commands on the local system
    /// </summary>
    class CommandHandler
    {
        private readonly List<string> _envPaths;

        private readonly List<string> _extensions;

        /// Initialize new object
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
        public string GetDefaultExe(Platform platform)
        {
            bool exists;
            string path;

            // Get Unix default shell
            if (platform is Platform.Nix)
            {
                (exists, path) = ExistsOnPath("bash");
                return exists ? path : "/bin/sh";
            }

            // Get Windows default shell
            (exists, path) = ExistsOnPath("powershell.exe");
            return exists ? path : GetExePath("cmd.exe");
        }

        /// Determine if executable exists on environment path
        public (bool exists, string) ExistsOnPath(string exe)
        {
            _ = exe ?? throw new ArgNullException(nameof(exe));
            string path = GetExePath(exe);

            if (path != null)
            {
                return (true, path);
            }

            // Try to resolve unspecified file extension
            if (!Path.HasExtension(exe))
            {
                foreach (string ext in _extensions)
                {
                    string name = Path.ChangeExtension(exe, ext);

                    if ((path = GetExePath(name)) != null)
                    {
                        return (true, path);
                    }
                }
            }
            return (false, null);
        }

        /// Search environment path for specified shell
        public string GetExePath(string exe)
        {
            string path = exe ?? throw new ArgNullException(nameof(exe));

            // File was found w/o searching env path
            if (File.Exists(path = Path.GetFullPath(path)))
            {
                return path;
            }

            // Search env path for executable
            foreach (string envPath in _envPaths)
            {
                path = Path.Combine(envPath, exe);

                if (File.Exists(path = Path.GetFullPath(path)))
                {
                    return path;
                }
            }
            return null;
        }
    }
}
