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
    class ErrorHandler
    {
        private readonly Status _status;

        private readonly List<Error> _errors;

        /// Initialize new object
        public ErrorHandler()
        {
            _status = new Status(ConsoleColor.Red, "error", "[x]");
            _errors = GetErrors();
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(Except type, string arg, Exception ex = null)
        {
            Handle(type, arg, false, ex);
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(Except type, string arg, bool showUsage,
                                                    Exception ex = null) {
            int index = IndexOfError(type);

            // Ensure error message is built
            if ((arg == null) && !_errors[index].Built)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            // Display program usage
            if (showUsage)
            {
                Console.WriteLine(ArgumentParser.GetUsage());
            }

            Console.ForegroundColor = _status.Color;
            Console.Write($"{_status.Symbol} ");
            Console.ResetColor();

            _errors[index].Build(arg);
            Console.WriteLine(_errors[index].Message);

            // Print exception info and/or exit message
            if (Program.Debug && (ex != null))
            {
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
        private int IndexOfError(Except errorType)
        {
            // Error list query
            List<int> query = (from error in _errors
                               where error.TypeName == errorType
                               select _errors.IndexOf(error)).ToList();

            return (query.Count() > 0) ? query[0] : -1;
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
                         "Unable to connect to {}"),
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
                new Error(Except.ExecPath,
                         "Unable to locate executable file '{}'"),
                new Error(Except.ExecProcess,
                         "Unable to launch executable process {}"),
                new Error(Except.SocketBind,
                         "The endpoint {} is already in use"),
                new Error(Except.Unhandled,
                         "Unhandled exception occurred: {}"),
                new Error(Except.UnknownArgs,
                         "Received unknown argument(s): {}")
            };
        }
    }
}
