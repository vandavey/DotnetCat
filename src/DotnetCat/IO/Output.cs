using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DotnetCat.Errors;
using static DotnetCat.IO.Constants;

namespace DotnetCat.IO;

/// <summary>
///  Console output utility class.
/// </summary>
internal static class Output
{
    /// <summary>
    ///  Write the given error message to the standard error stream.
    /// </summary>
    public static void Error([NotNull] string? msg) => Log(msg, LogLevel.Error);

    /// <summary>
    ///  Write the given status or completion message to the standard output stream.
    /// </summary>
    public static void Status([NotNull] string? msg) => Log(msg, LogLevel.Status);

    /// <summary>
    ///  Write the given message to the standard console
    ///  stream corresponding to the specified logging level.
    /// <summary>
    public static void Log([NotNull] string? msg, LogLevel level = default)
    {
        string msgPrefix = Sequence.Colorize(LogPrefix(level), PrefixColor(level));
        ConsoleStream(level).WriteLine($"{msgPrefix} {ThrowIf.NullOrEmpty(msg)}");
    }

    /// <summary>
    ///  Get the log message prefix symbol corresponding to the given logging level.
    /// </summary>
    private static string LogPrefix(LogLevel level) => ThrowIf.Undefined(level) switch
    {
        LogLevel.Status => STATUS_LOG_PREFIX,
        LogLevel.Warn   => WARNING_LOG_PREFIX,
        LogLevel.Error  => ERROR_LOG_PREFIX,
        _               => INFO_LOG_PREFIX
    };

    /// <summary>
    ///  Get the log message prefix symbol console color
    ///  corresponding to the given logging level.
    /// </summary>
    private static ConsoleColor PrefixColor(LogLevel level)
    {
        return ThrowIf.Undefined(level) switch
        {
            LogLevel.Status => ConsoleColor.Green,
            LogLevel.Warn   => ConsoleColor.Yellow,
            LogLevel.Error  => ConsoleColor.Red,
            _               => ConsoleColor.Cyan
        };
    }

    /// <summary>
    ///  Get the standard console stream corresponding to the given logging level.
    /// </summary>
    private static TextWriter ConsoleStream(LogLevel level)
    {
        ThrowIf.Undefined(level);
        return level is LogLevel.Error or LogLevel.Warn ? Console.Error : Console.Out;
    }
}
