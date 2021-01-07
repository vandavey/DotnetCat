using System;
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
    static class CommandHandler
    {
        private static readonly string[] _clearCommands;

        private static readonly List<string> _envPaths;

        private static readonly List<string> _extensions;

        /// Initialize static members
        static CommandHandler()
        {
            string path = Env.GetEnvironmentVariable("PATH");
            _envPaths = path.Split(Path.PathSeparator).ToList();

            _clearCommands = new string[]
            {
                "cls", "clear", "clear-screen"
            };

            _extensions = new List<string>
            {
                "exe", "bat", "ps1", "py", "sh"
            };
        }

        /// Get default command shell for the platform
        public static string GetDefaultExe(Platform platform)
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
        public static (bool exists, string path) ExistsOnPath(string exe)
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

        /// Determine if data contains clear command
        public static bool IsClearCmd(string data, bool doClear = true)
        {
            data = data.Replace(Env.NewLine, "").Trim();

            // Clear current console buffer
            if (_clearCommands.Contains(data))
            {
                if (doClear)
                {
                    Console.Clear();
                }
                return true;
            }
            return false;
        }

        /// Search environment path for specified shell
        public static string GetExePath(string exe)
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
