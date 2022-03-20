using System;
using System.Diagnostics;
using System.Linq;
using DotnetCat.IO;
using DotnetCat.IO.FileSystem;

namespace DotnetCat.Shell.Commands
{
    /// <summary>
    ///  Command and executable process controller
    /// </summary>
    internal static class Command
    {
        private static readonly string[] _clsCommands;  // Clear commands

        /// <summary>
        ///  Initialize static members
        /// </summary>
        static Command()
        {
            _clsCommands = new string[] { "cls", "clear", "clear-host" };
        }

        /// <summary>
        ///  Get the value of the specified environment variable
        /// </summary>
        public static string? GetEnvVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        /// <summary>
        ///  Get ProcessStartInfo to use for executable startup
        /// </summary>
        public static ProcessStartInfo GetExeStartInfo(string? shell)
        {
            _ = shell ?? throw new ArgumentNullException(nameof(shell));

            // Exe process startup information
            ProcessStartInfo info = new(shell)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = FileSys.GetUserHomePath()
            };

            // Profile loading only supported on Windows
            if (OperatingSystem.IsWindows())
            {
                info.LoadUserProfile = true;
            }
            return info;
        }

        /// <summary>
        ///  Determine if the data contains a clear command
        /// </summary>
        public static bool IsClearCmd(string data, bool doClear = true)
        {
            bool isClear = false;
            data = data.Replace(Environment.NewLine, string.Empty).Trim();

            // Clear command detected
            if (_clsCommands.Contains(data))
            {
                if (doClear)  // Clear console buffer
                {
                    Sequence.ClearScreen();
                }
                isClear = true;
            }
            return isClear;
        }
    }
}
