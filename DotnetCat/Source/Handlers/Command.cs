using System;
using System.IO;
using System.Linq;
using ArgNullException = System.ArgumentNullException;
using Env = System.Environment;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Command and executable process handler
    /// </summary>
    internal static class Command
    {
        private static readonly string[] _clsCommands;  // Clear commands

        private static readonly string[] _envPaths;     // Environment path

        private static readonly string[] _exeFiles;     // Executable files

        /// <summary>
        /// Initialize static members
        /// </summary>
        static Command()
        {
            string path = Env.GetEnvironmentVariable("PATH");
            _envPaths = path.Split(Path.PathSeparator);

            _clsCommands = new string[] { "cls", "clear", "clear-host" };
            _exeFiles = new string[] { "exe", "bat", "ps1", "py", "sh" };
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
                foreach (string ext in _exeFiles)
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
            data = data.Replace(Env.NewLine, string.Empty).Trim();

            // Clear command detected
            if (_clsCommands.Contains(data))
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
