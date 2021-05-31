using System;
using System.Collections.Generic;
using DotnetCat.Enums;
using DotnetCat.Utils;
using Env = System.Environment;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Handler for custom DotnetCat errors
    /// </summary>
    static class Error
    {
        // Error message dictionary
        private static readonly Dictionary<Except, ErrorMessage> _errors;

        /// <summary>
        /// Initialize static members
        /// </summary>
        static Error() => _errors = GetErrorDict();

        /// <summary>
        /// Handle special exceptions related to DotNetCat
        /// </summary>
        public static void Handle(Except exType,
                                  string arg,
                                  Exception ex = default) {
            // Call overload
            Handle(exType, arg, false, ex);
        }

        /// <summary>
        /// Handle special exceptions related to DotNetCat
        /// </summary>
        public static void Handle(Except exType,
                                  string arg,
                                  Exception ex,
                                  Level level = default) {
            // Call overload
            Handle(exType, arg, false, ex, level);
        }

        /// <summary>
        /// Handle special exceptions related to DotNetCat
        /// </summary>
        public static void Handle(Except exType,
                                  string arg,
                                  bool showUsage,
                                  Exception ex = default,
                                  Level level = default) {
            // Unknown error
            if (!_errors.ContainsKey(exType))
            {
                throw new ArgumentException(null, nameof(exType));
            }
            _ = arg ?? throw new ArgumentNullException(nameof(arg));

            // Display program usage
            if (showUsage)
            {
                Console.WriteLine(Parser.Usage);
            }
            _errors[exType].Build(arg);

            // Print warning/error message
            if (level is Level.Warn)
            {
                Style.Warn(_errors[exType].Value);
            }
            else
            {
                Style.Error(_errors[exType].Value);
            }

            // Print debug information
            if (Program.Debug && (ex is not null))
            {
                ex = (ex is AggregateException) ? ex.InnerException : ex;
                string header = $"----[ {ex.GetType().FullName} ]----";

                Console.WriteLine(string.Join(Env.NewLine, new string[]
                {
                    header,
                    ex.ToString(),
                    new string('-', header.Length)
                }));
            }

            Console.WriteLine();
            Env.Exit(1);
        }

        /// <summary>
        /// Get dictionary of errors related to DotnetCat
        /// </summary>
        private static Dictionary<Except, ErrorMessage> GetErrorDict() => new()
        {
            {
                Except.ArgsCombo,
                new ErrorMessage("Invalid argument combination: {}")
            },
            {
                Except.ConnectionLost,
                new ErrorMessage("Connection unexpectedly closed by {}")
            },
            {
                Except.ConnectionRefused,
                new ErrorMessage("Connection was actively refused by {}")
            },
            {
                Except.ConnectionTimeout,
                new ErrorMessage("Socket timeout occurred: {}")
            },
            {
                Except.DirectoryPath,
                new ErrorMessage("Unable to locate parent directory '{}'")
            },
            {
                Except.EmptyPath,
                new ErrorMessage("A value is required for option(s): {}")
            },
            {
                Except.ExePath,
                new ErrorMessage("Unable to locate executable file '{}'")
            },
            {
                Except.ExeProcess,
                new ErrorMessage("Unable to launch executable process: {}")
            },
            {
                Except.FilePath,
                new ErrorMessage("Unable to locate file path '{}'")
            },
            {
                Except.InvalidAddr,
                new ErrorMessage("Unable to resolve hostname '{}'")
            },
            {
                Except.InvalidArgs,
                new ErrorMessage("Unable to validate argument(s): {}")
            },
            {
                Except.InvalidPort,
                new ErrorMessage("'{}' is not a valid port number")
            },
            {
                Except.NamedArgs,
                new ErrorMessage("Missing value for named argument(s): {}")
            },
            {
                Except.Payload,
                new ErrorMessage("Invalid payload for argument(s): {}")
            },
            {
                Except.RequiredArgs,
                new ErrorMessage("Missing required argument(s): {}")
            },
            {
                Except.SocketBind,
                new ErrorMessage("The endpoint is already in use: {}")
            },
            {
                Except.StringEOL,
                new ErrorMessage("Missing EOL in argument(s): {}")
            },
            {
                Except.Unhandled,
                new ErrorMessage("Unhandled exception occurred: {}")
            },
            {
                Except.UnknownArgs,
                new ErrorMessage("Received unknown argument(s): {}")
            }
        };
    }
}
