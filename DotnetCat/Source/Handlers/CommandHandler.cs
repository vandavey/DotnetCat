using System;
using System.IO;
using System.Linq;
using DotnetCat.Enums;
using ArgNullException = System.ArgumentNullException;
using Env = System.Environment;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Command and executable process handler
    /// </summary>
    static class CommandHandler
    {
        private static readonly string[] _clearCommands;

        private static readonly string[] _envPaths;

        private static readonly string[] _extensions;

        /// <summary>
        /// Initialize static members
        /// </summary>
        static CommandHandler()
        {
            string path = Env.GetEnvironmentVariable("PATH");
            _envPaths = path.Split(Path.PathSeparator);

            _clearCommands = new string[]
            {
                "cls", "clear", "clear-screen"
            };

            _extensions = new string[]
            {
                "exe", "bat", "ps1", "py", "sh"
            };
        }

        /// <summary>
        /// Get default command shell for the platform
        /// </summary>
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

        /// <summary>
        /// Determine if executable exists on environment path
        /// </summary>
        public static (bool exists, string path) ExistsOnPath(string exe)
        {
            _ = exe ?? throw new ArgNullException(nameof(exe));
            string path = GetExePath(exe);

            if (path is not null)
            {
                return (true, path);
            }

            // Try to resolve unspecified file extension
            if (!Path.HasExtension(exe))
            {
                foreach (string ext in _extensions)
                {
                    string name = Path.ChangeExtension(exe, ext);

                    if ((path = GetExePath(name)) is not null)
                    {
                        return (true, path);
                    }
                }
            }
            return (false, null);
        }

        /// <summary>
        /// Determine if data contains clear command
        /// </summary>
        public static bool IsClearCmd(string data, bool doClear = true)
        {
            data = data.Replace(Env.NewLine, "").Trim();

            // Clear command detected
            if (_clearCommands.Contains(data))
            {
                if (doClear)  // Clear console buffer
                {
                    Console.Clear();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Search environment path for specified shell
        /// </summary>
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
