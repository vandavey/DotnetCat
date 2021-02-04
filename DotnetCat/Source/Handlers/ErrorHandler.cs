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
        private static readonly List<Error> _errors;

        /// Initialize static members
        static ErrorHandler()
        {
            _errors = GetErrors();
        }

        /// Handle special exceptions related to DotNetCat
        public static void Handle(Except type, string arg,
                                               Exception ex = null) {
            Handle(type, arg, false, ex);
        }

        /// Handle special exceptions related to DotNetCat
        public static void Handle(Except type, string arg,
                                               Exception ex,
                                               Level level = Level.Error) {
            Handle(type, arg, false, ex, level);
        }

        /// Handle special exceptions related to DotNetCat
        public static void Handle(Except type, string arg,
                                               bool showUsage,
                                               Exception ex = null,
                                               Level level = Level.Error) {
            int index = IndexOfError(type);

            if (index == -1)  // Index out of bounds
            {
                throw new IndexOutOfRangeException($"Invalid index: {index}");
            }

            if ((arg == null) && !_errors[index].Built)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            if (showUsage)  // Display program usage
            {
                Console.WriteLine(ArgumentParser.GetUsage());
            }
            _errors[index].Build(arg);

            if (level is Level.Warn)  // Print warning message
            {
                StyleHandler.Warn(_errors[index].Message);
            }
            else
            {
                StyleHandler.Error(_errors[index].Message);
            }

            // Print debug info and exit
            if (Program.Debug && (ex != null))
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
            else
            {
                Console.WriteLine();
            }
            Env.Exit(1);
        }

        /// Get the index of an error in Errors
        private static int IndexOfError(Except type)
        {
            Error status = _errors.Where(e => e.TypeName == type).First();
            return _errors.IndexOf(status);
        }

        /// Get errors related to DotnetCat
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
                new Error(Except.FilePath,
                          "Unable to locate file path '{}'"),
                new Error(Except.InvalidAddr,
                          "Unable to resolve hostname '{}'"),
                new Error(Except.InvalidPort,
                          "{} cannot be parsed as a valid port"),
                new Error(Except.NamedArgs,
                          "Missing value for named argument(s): {} "),
                new Error(Except.RequiredArgs,
                          "Missing required argument(s): {}"),
                new Error(Except.ExePath,
                          "Unable to locate executable file '{}'"),
                new Error(Except.ExeProcess,
                          "Unable to launch executable process: {}"),
                new Error(Except.SocketBind,
                          "The endpoint is already in use: {}"),
                new Error(Except.Unhandled,
                          "Unhandled exception occurred: {}"),
                new Error(Except.UnknownArgs,
                          "Received unknown argument(s): {}")
            };
        }
    }
}
