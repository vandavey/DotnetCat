using System;
using System.Collections.Generic;
using System.Linq;
using Prog = DotnetCat.Program;
using DotnetCat.Nodes;
using DotnetCat.Utils;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Handler for custom errors/error statuses
    /// </summary>
    class ErrorHandler
    {
        private readonly Status _status;

        private readonly StyleHandler _style;

        private readonly List<Error> _errors;

        /// Initialize new ErrorHandler
        public ErrorHandler()
        {
            _style = new StyleHandler();
            _status = new Status("error", "[x]", ConsoleColor.Red);

            _errors = new List<Error>
            {
                new Error("address", "{} cannot be parsed as an IPv4 address"),
                new Error("bind", "The endpoint {} is already in use"),
                new Error("closed", "The connection was closed by {}"),
                new Error("dirpath", "Unable to locate parent directory {}"),
                new Error("emptypath", "A value is required for option {}"),
                new Error("filepath", "Unable to locate file path {}"),
                new Error("flag", "Missing value for named argument(s): {} "),
                new Error("port", "{} cannot be parsed as a valid port"),
                new Error("process", "Unable to run the process {}"),
                new Error("required", "Missing required argument(s): {}"),
                new Error("shell", "Unable to locate the shell executable {}"),
                new Error("socket", "Unable to connect to {}"),
                new Error("unknown", "Received unknown argument(s): {}"),
                new Error("validation", "Unable to validate argument(s): {}")
            };
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(string name, string arg)
        {
            Handle(name, arg, false);
        }

        /// Handle special exceptions related to DotNetCat
        public void Handle(string name, string arg, bool showUsage)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            int index = IndexOfError(name);

            if (index == -1)
            {
                throw new ArgumentException("Invalid error name");
            }

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

            if (Prog.IsVerbose)
            {
                _style.Status("Exiting DotnetCat");
            }

            Console.WriteLine();
            Environment.Exit(1);
        }

        /// Get the index of an error in Errors
        private int IndexOfError(string name)
        {
            int index = -1;

            List<int> query = (from error in _errors
                               where error.Name.ToLower() == name.ToLower()
                               select _errors.IndexOf(error)).ToList();

            query.ForEach(x => index = x);
            return index;
        }
    }
}
