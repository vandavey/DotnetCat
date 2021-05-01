using System;
using System.Collections.Generic;
using System.Linq;
using DotnetCat.Enums;
using DotnetCat.Utils;
using Env = System.Environment;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Handler for custom DotnetCat errors
    /// </summary>
    static class ErrorHandler
    {
        private static readonly List<Error> _errors;  // Error list

        /// <summary>
        /// Initialize static members
        /// </summary>
        static ErrorHandler() => _errors = GetErrors();

        /// <summary>
        /// Handle special exceptions related to DotNetCat
        /// </summary>
        public static void Handle(Except type, string arg,
                                               Exception ex = default) {
            Handle(type, arg, false, ex);
        }

        /// <summary>
        /// Handle special exceptions related to DotNetCat
        /// </summary>
        public static void Handle(Except type, string arg,
                                               Exception ex,
                                               Level level = default) {
            Handle(type, arg, false, ex, level);
        }

        /// <summary>
        /// Handle special exceptions related to DotNetCat
        /// </summary>
        public static void Handle(Except type, string arg,
                                               bool showUsage,
                                               Exception ex = default,
                                               Level level = default) {
            int index = IndexOfError(type);

            // Index out of bounds
            if (index == -1)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            if ((arg is null) && !_errors[index].Built)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            // Display program usage
            if (showUsage)
            {
                Console.WriteLine(ArgumentParser.GetUsage());
            }
            _errors[index].Build(arg);

            // Print warning/error message
            if (level is Level.Warn)
            {
                StyleHandler.Warn(_errors[index].Message);
            }
            else
            {
                StyleHandler.Error(_errors[index].Message);
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
        /// Get the index of an error in Errors
        /// </summary>
        private static int IndexOfError(Except type)
        {
            Error status = _errors.Where(e => e.ExceptType == type).First();
            return _errors.IndexOf(status);
        }

        /// <summary>
        /// Get errors related to DotnetCat
        /// </summary>
        private static List<Error> GetErrors() => new()
        {
            new(Except.ArgsCombo, "The arguments can't be combined: {}"),
            new(Except.InvalidArgs, "Unable to validate argument(s): {}"),
            new(Except.ConnectionLost, "Connection unexpectedly closed by {}"),
            new(Except.ConnectionRefused, "Connection to {} was refused"),
            new(Except.ConnectionTimeout, "Socket timeout occurred: {}"),
            new(Except.DirectoryPath, "Unable to locate parent directory '{}'"),
            new(Except.EmptyPath, "A value is required for option(s): {}"),
            new(Except.ExePath, "Unable to locate executable file '{}'"),
            new(Except.ExeProcess, "Unable to launch executable process: {}"),
            new(Except.FilePath, "Unable to locate file path '{}'"),
            new(Except.InvalidAddr, "Unable to resolve hostname '{}'"),
            new(Except.InvalidPort, "{} cannot be parsed as a valid port"),
            new(Except.NamedArgs, "Missing value for named argument(s): {}"),
            new(Except.Payload, "Invalid payload for argument(s): {}"),
            new(Except.RequiredArgs, "Missing required argument(s): {}"),
            new(Except.SocketBind, "The endpoint is already in use: {}"),
            new(Except.StringEOL, "Missing EOL in argument(s): {}"),
            new(Except.Unhandled, "Unhandled exception occurred: {}"),
            new(Except.UnknownArgs, "Received unknown argument(s): {}")
        };
    }
}
