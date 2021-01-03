using System;
using System.Collections.Generic;
using System.Linq;
using DotnetCat.Enums;
using DotnetCat.Utils;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Handler for custom DotnetCat errors
    /// </summary>
    class ErrorHandler
    {
        private readonly Status _status;

        private readonly StyleHandler _style;

        private readonly List<Error> _errors;

        /// Initialize new object
        public ErrorHandler()
        {
            _errors = GetErrors();
            _status = new Status("error", "[x]", ConsoleColor.Red);
            _style = new StyleHandler();
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(Except type, string arg)
        {
            Handle(type, arg, false);
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(Except type, string arg, bool showUsage)
        {
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

            if (Program.Verbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(1);
        }

        /// Get the index of an error in Errors
        private int IndexOfError(Except errorType)
        {
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
                new Error(Except.ArgCombination,
                         "The following arguments can't be combined: {}"),
                new Error(Except.ArgValidation,
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
                new Error(Except.NamedArg,
                         "Missing value for named argument(s): {} "),
                new Error(Except.RequiredArg,
                         "Missing required argument(s): {}"),
                new Error(Except.ShellPath,
                         "Unable to locate the shell executable {}"),
                new Error(Except.ShellProcess,
                         "Unable to run the process {}"),
                new Error(Except.SocketBind,
                         "The endpoint {} is already in use"),
                new Error(Except.UnknownArg,
                         "Received unknown argument(s): {}"),
            };
        }
    }
}
