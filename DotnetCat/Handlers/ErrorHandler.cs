using System;
using System.Collections.Generic;
using System.Linq;
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

        /// Initialize new ErrorHandler
        public ErrorHandler()
        {
            _errors = GetErrors();
            _status = new Status("error", "[x]", ConsoleColor.Red);
            _style = new StyleHandler();
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(ErrorType type, string arg)
        {
            Handle(type, arg, false);
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(ErrorType type, string arg, bool showUsage)
        {
            int index = IndexOfError(type);

            if ((arg == null) && !_errors[index].IsBuilt)
            {
                throw new ArgumentNullException("arg");
            }

            if (showUsage)
            {
                Console.WriteLine(ArgumentParser.GetUsage());
            }

            Console.ForegroundColor = _status.Color;
            Console.Write($"{_status.Symbol} ");
            Console.ResetColor();

            _errors[index].Build(arg);
            Console.WriteLine(_errors[index].Message);

            if (Program.IsVerbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(1);
        }

        /// Get the index of an error in Errors
        private int IndexOfError(ErrorType errorType)
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
                new Error(ErrorType.ArgCombination,
                    msg: "The following arguments can't be combined: {}"),
                new Error(ErrorType.ArgValidation,
                    msg: "Unable to validate argument(s): {}"),
                new Error(ErrorType.ConnectionLost,
                    msg: "The connection was unexpectedly closed by {}'"),
                new Error(ErrorType.ConnectionRefused,
                    msg: "Unable to connect to {}"),
                new Error(ErrorType.DirectoryPath,
                    msg: "Unable to locate parent directory '{}'"),
                new Error(ErrorType.EmptyPath,
                    msg: "A value is required for option(s): {}"),
                new Error(ErrorType.FilePath,
                    msg: "Unable to locate file path '{}'"),
                new Error(ErrorType.InvalidAddress,
                    msg: "Unable to resolve hostname {}"),
                new Error(ErrorType.InvalidPort,
                    msg: "{} cannot be parsed as a valid port"),
                new Error(ErrorType.NamedArg,
                    msg: "Missing value for named argument(s): {} "),
                new Error(ErrorType.RequiredArg,
                    msg: "Missing required argument(s): {}"),
                new Error(ErrorType.ShellPath,
                    msg: "Unable to locate the shell executable {}"),
                new Error(ErrorType.ShellProcess,
                    msg: "Unable to run the process {}"),
                new Error(ErrorType.SocketBind,
                    msg: "The endpoint {} is already in use"),
                new Error(ErrorType.UnknownArg,
                    msg: "Received unknown argument(s): {}"),
            };
        }
    }
}
