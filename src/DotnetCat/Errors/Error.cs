using System;
using DotnetCat.IO;
using DotnetCat.Utils;

namespace DotnetCat.Errors
{
    /// <summary>
    ///  Error and exception utility class.
    /// </summary>
    internal static class Error
    {
        /// <summary>
        ///  Initialize the static class members.
        /// </summary>
        static Error() => Debug = false;

        /// Enable verbose exceptions
        public static bool Debug { get; set; }

        /// <summary>
        ///  Handle user-defined exceptions related to DotnetCat.
        /// </summary>
        public static void Handle(Except exType,
                                  string? arg,
                                  Exception? ex = default) {

            Handle(exType, arg, false, ex);
        }

        /// <summary>
        ///  Handle user-defined exceptions related to DotnetCat.
        /// </summary>
        public static void Handle(Except exType,
                                  string? arg,
                                  Exception? ex,
                                  Level level = default) {

            Handle(exType, arg, false, ex, level);
        }

        /// <summary>
        ///  Handle user-defined exceptions related to DotnetCat.
        /// </summary>
        public static void Handle(Except exType,
                                  string? arg,
                                  bool showUsage,
                                  Exception? ex = default,
                                  Level level = default) {

            _ = arg ?? throw new ArgumentNullException(nameof(arg));

            // Display program usage
            if (showUsage)
            {
                Console.WriteLine(Parser.Usage);
            }

            ErrorMessage errorMsg = GetErrorMessage(exType);
            errorMsg.Build(arg);

            // Print warning/error message
            if (level is Level.Warn)
            {
                Style.Warn(errorMsg.Message);
            }
            else
            {
                Style.Error(errorMsg.Message);
            }

            // Print debug information
            if (Debug && ex is not null)
            {
                if (ex is AggregateException aggregateEx)
                {
                    ex = aggregateEx.InnerException;
                }

                string header = $"----[ {ex?.GetType().FullName} ]----";

                Console.WriteLine(string.Join(Environment.NewLine, new[]
                {
                    header,
                    ex?.ToString() ?? string.Empty,
                    new string('-', header.Length)
                }));
            }

            Console.WriteLine();
            Environment.Exit(1);
        }

        /// <summary>
        ///  Get a new error message that corresponds to the given
        ///  exception enumeration type.
        /// </summary>
        private static ErrorMessage GetErrorMessage(Except exType)
        {
            return new ErrorMessage(GetErrorFormatMessage(exType));
        }

        /// <summary>
        ///  Get the raw error message string that corresponds to the
        ///  given exception enumeration type.
        /// </summary>
        private static string GetErrorFormatMessage(Except exType) => exType switch
        {
            Except.ArgsCombo         => "Invalid argument combination: %",
            Except.ConnectionLost    => "Connection unexpectedly closed by %",
            Except.ConnectionRefused => "Connection was actively refused by %",
            Except.ConnectionTimeout => "Socket timeout occurred: %",
            Except.DirectoryPath     => "Unable to locate parent directory '%'",
            Except.EmptyPath         => "A value is required for option(s): %",
            Except.ExePath           => "Unable to locate executable file '%'",
            Except.ExeProcess        => "Unable to launch executable process: %",
            Except.FilePath          => "Unable to locate file path '%'",
            Except.InvalidAddr       => "Unable to resolve hostname '%'",
            Except.InvalidArgs       => "Unable to validate argument(s): %",
            Except.InvalidPort       => "'%' is not a valid port number",
            Except.NamedArgs         => "Missing value for named argument(s): %",
            Except.RequiredArgs      => "Invalid payload for argument(s): %",
            Except.Payload           => "Missing required argument(s): %",
            Except.SocketBind        => "The endpoint is already in use: %",
            Except.StringEOL         => "Missing EOL in argument(s): %",
            Except.UnknownArgs       => "Received unknown argument(s): %",
            Except.Unhandled or _    => "Unhandled exception occurred: %",
        };
    }
}
