using System;
using System.Diagnostics;
using System.Linq;
using DotnetCat.IO;
using DotnetCat.IO.FileSystem;
using DotnetCat.Utils;

namespace DotnetCat.Shell.Commands
{
    /// <summary>
    ///  Command and executable process utility class.
    /// </summary>
    internal static class Command
    {
        private static readonly string[] _clsCommands;  // Clear commands

        /// <summary>
        ///  Initialize the static class members.
        /// </summary>
        static Command() => _clsCommands = new[]
        {
            "cls",
            "clear",
            "clear-host"
        };

        /// <summary>
        ///  Get the value of the given environment variable.
        /// </summary>
        public static string? GetEnvVariable(string varName)
        {
            return Environment.GetEnvironmentVariable(varName);
        }

        /// <summary>
        ///  Get process startup information to initialize the given command shell.
        /// </summary>
        public static ProcessStartInfo GetExeStartInfo(string? shell)
        {
            _ = shell ?? throw new ArgumentNullException(nameof(shell));

            ProcessStartInfo startInfo = new(shell)
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
                startInfo.LoadUserProfile = true;
            }
            return startInfo;
        }

        /// <summary>
        ///  Determine whether the given data contains a clear command.
        /// </summary>
        public static bool IsClearCmd(string data)
        {
            bool isClear = false;

            if (!data.IsNullOrEmpty())
            {
                data = data.Replace(Environment.NewLine, string.Empty).Trim();
                isClear = _clsCommands.Contains(data.ToLower());
            }
            return isClear;
        }
    }
}
