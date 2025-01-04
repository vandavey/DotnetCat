using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.IO;
using DotnetCat.Network;
using DotnetCat.Utils;

namespace DotnetCat.Errors;

/// <summary>
///  Error and exception utility class.
/// </summary>
internal static class Error
{
    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Error() => Verbose = false;

    /// <summary>
    ///  Enable verbose exceptions.
    /// </summary>
    public static bool Verbose { get; set; }

    /// <summary>
    ///  Handle user-defined exceptions related to DotnetCat and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType,
                              [NotNull] string? arg,
                              Exception? ex = default)
    {
        Handle(exType, arg, false, ex);
    }

    /// <summary>
    ///  Handle user-defined exceptions related to DotnetCat and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType,
                              [NotNull] string? arg,
                              bool showUsage,
                              Exception? ex = default)
    {
        ThrowIf.NullOrEmpty(arg);

        // Print application usage
        if (showUsage)
        {
            Console.WriteLine(Parser.Usage);
        }

        ErrorMessage errorMsg = MakeErrorMessage(exType, arg);
        Output.Error(errorMsg.Message);

        // Print verbose error details
        if (Verbose && ex is not null)
        {
            if (ex is AggregateException aggregateEx)
            {
                ex = Net.SocketException(aggregateEx) ?? ex;
            }

            string errorName = Sequence.Colorize(ex.GetType().FullName, ConsoleColor.Red);
            string header = $"----[ {errorName} ]----";

            Console.WriteLine($"""
                {header}
                {ex?.ToString()}
                {new string('-', Sequence.Length(header))}
                """
            );
        }

        Console.WriteLine();
        Environment.Exit(1);
    }

    /// <summary>
    ///  Get a new error message that corresponds to the given
    ///  exception enumeration type.
    /// </summary>
    private static ErrorMessage MakeErrorMessage(Except exType, string? arg = default)
    {
        ErrorMessage message = new(exType switch
        {
            Except.AddressInUse       => "The endpoint is already in use: %",
            Except.ArgsCombo          => "Invalid argument combination: %",
            Except.ConnectionAborted  => "Local software aborted connection to %",
            Except.ConnectionRefused  => "Connection was actively refused by %",
            Except.ConnectionReset    => "Connection was reset by %",
            Except.DirectoryPath      => "Unable to locate parent directory: '%'",
            Except.EmptyPath          => "A value is required for option(s): %",
            Except.ExePath            => "Unable to locate executable file: '%'",
            Except.ExeProcess         => "Unable to launch executable process: %",
            Except.FilePath           => "Unable to locate file: '%'",
            Except.HostNotFound       => "Unable to resolve hostname: '%'",
            Except.HostUnreachable    => "Unable to reach host %",
            Except.InvalidArgs        => "Unable to validate argument(s): %",
            Except.InvalidPort        => "'%' is not a valid port number",
            Except.NamedArgs          => "Missing value for named argument(s): %",
            Except.NetworkDown        => "The network is down: %",
            Except.NetworkReset       => "Connection to % was lost in network reset",
            Except.NetworkUnreachable => "The network is unreachable: %",
            Except.Payload            => "Invalid payload data: %",
            Except.RequiredArgs       => "Missing required argument(s): %",
            Except.SocketError        => "Unspecified socket error occurred: %",
            Except.StringEol          => "Missing EOL in argument(s): %",
            Except.TimedOut           => "Socket timeout occurred: %",
            Except.UnknownArgs        => "One or more unknown arguments: %",
            Except.Unhandled or _     => "Unhandled exception occurred: %"
        });

        if (!arg.IsNullOrEmpty())
        {
            message.Build(arg);
        }
        return message;
    }
}
