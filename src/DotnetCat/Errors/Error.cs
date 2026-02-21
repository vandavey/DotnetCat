using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.IO;
using DotnetCat.Network;
using DotnetCat.Utils;
using static DotnetCat.Errors.Constants;
using static DotnetCat.Utils.Constants;

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
    ///  Handle a DotnetCat error and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType,
                              [NotNull] string? arg,
                              Exception? ex = default)
    {
        Handle(exType, arg, false, ex);
    }

    /// <summary>
    ///  Handle a DotnetCat error and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType,
                              [NotNull] string? arg,
                              bool showUsage,
                              Exception? ex = default)
    {
        Console.Out.WriteLineIf(showUsage, APP_USAGE);
        Output.Error(MakeErrorMsg(exType, arg));

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
        Environment.Exit(ERROR_EXIT_CODE);
    }

    /// <summary>
    ///  Handle a DotnetCat error and exit if a specific condition is true.
    /// </summary>
    public static void HandleIf([DoesNotReturnIf(true)] bool condition,
                                Except exType,
                                [NotNull] string? arg,
                                bool showUsage,
                                Exception? ex = default)
    {
        if (condition)
        {
            Handle(exType, arg, showUsage, ex);
        }
        ThrowIf.NullOrEmpty(arg);
    }

    /// <summary>
    ///  Create an error message by interpolating the given argument
    ///  in the message corresponding to the given exception type.
    /// </summary>
    private static string MakeErrorMsg(Except exType, [NotNull] string? arg)
    {
        string errorMsg = ThrowIf.Undefined(exType) switch
        {
            Except.AddressInUse       => ADDRESS_IN_USE_ERROR,
            Except.ArgsCombo          => ARGS_COMBO_ERROR,
            Except.ConnectionAborted  => CONNECT_ABORTED_ERROR,
            Except.ConnectionRefused  => CONNECT_REFUSED_ERROR,
            Except.ConnectionReset    => CONNECT_RESET_ERROR,
            Except.DirectoryPath      => DIRECTORY_PATH_ERROR,
            Except.EmptyPath          => EMPTY_PATH_ERROR,
            Except.ExePath            => EXE_PATH_ERROR,
            Except.ExeProcess         => EXE_PROCESS_ERROR,
            Except.FilePath           => FILE_PATH_ERROR,
            Except.HostNotFound       => HOST_NOT_FOUND_ERROR,
            Except.HostUnreachable    => HOST_UNREACHABLE_ERROR,
            Except.InvalidArgs        => INVALID_ARGS_ERROR,
            Except.InvalidPort        => INVALID_PORT_ERROR,
            Except.NamedArgs          => NAMED_ARGS_ERROR,
            Except.NetworkDown        => NETWORK_DOWN_ERROR,
            Except.NetworkReset       => NETWORK_RESET_ERROR,
            Except.NetworkUnreachable => NETWORK_UNREACHABLE_ERROR,
            Except.Payload            => PAYLOAD_ERROR,
            Except.RequiredArgs       => REQUIRED_ARGS_ERROR,
            Except.SocketError        => SOCKET_ERROR_ERROR,
            Except.StringEol          => STRING_EOL_ERROR,
            Except.TimedOut           => TIMED_OUT_ERROR,
            Except.UnknownArgs        => UNKNOWN_ARGS_ERROR,
            _                         => UNHANDLED_ERROR
        };
        return errorMsg.Replace("%", ThrowIf.NullOrEmpty(arg));
    }
}
