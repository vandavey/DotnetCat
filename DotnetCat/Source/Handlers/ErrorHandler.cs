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
        private static List<Error> GetErrors()
        {
            return new List<Error>
            {
                new Error(Except.ArgsCombo,
                          "The following arguments can't be combined: {}"),
                new Error(Except.InvalidArgs,
                          "Unable to validate argument(s): {}"),
                new Error(Except.ConnectionLost,
                          "The connection was unexpectedly closed by {}"),
                new Error(Except.ConnectionRefused,
                          "Connection to {} was refused"),
                new Error(Except.ConnectionTimeout,
                          "Socket timeout occurred: {}"),
                new Error(Except.DirectoryPath,
                          "Unable to locate parent directory '{}'"),
                new Error(Except.EmptyPath,
                          "A value is required for option(s): {}"),
                new Error(Except.ExePath,
                          "Unable to locate executable file '{}'"),
                new Error(Except.ExeProcess,
                          "Unable to launch executable process: {}"),
                new Error(Except.FilePath,
                          "Unable to locate file path '{}'"),
                new Error(Except.InvalidAddr,
                          "Unable to resolve hostname '{}'"),
                new Error(Except.InvalidPort,
                          "{} cannot be parsed as a valid port"),
                new Error(Except.NamedArgs,
                          "Missing value for named argument(s): {}"),
                new Error(Except.Payload,
                          "Invalid payload for argument(s): {}"),
                new Error(Except.RequiredArgs,
                          "Missing required argument(s): {}"),
                new Error(Except.SocketBind,
                          "The endpoint is already in use: {}"),
                new Error(Except.StringEOL,
                          "Missing string EOL in argument(s): {}"),
                new Error(Except.Unhandled,
                          "Unhandled exception occurred: {}"),
                new Error(Except.UnknownArgs,
                          "Received unknown argument(s): {}")
            };
        }
    }
}
