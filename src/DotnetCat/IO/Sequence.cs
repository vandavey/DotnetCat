using System;
using DotnetCat.Shell.WinApi;
using DotnetCat.Utils;

namespace DotnetCat.IO;

/// <summary>
///  Virtual terminal escape sequence utility class.
/// </summary>
internal static class Sequence
{
    private const string ESCAPE = "\x1b";

    private const string CLEAR = $"{ESCAPE}[H{ESCAPE}[2J{ESCAPE}[3J";

    private const string RESET = $"{ESCAPE}[0m";

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Sequence() => ConsoleApi.EnableVirtualTerm();

    /// <summary>
    ///  Clear the current console screen buffer.
    /// </summary>
    public static void ClearScreen() => Console.Write(CLEAR);

    /// <summary>
    ///  Get the ANSI foreground color SGR escape sequence that
    ///  corresponds to the given console color.
    /// </summary>
    public static string GetColorStr(ConsoleColor color)
    {
        return GetColorSequence(color);
    }

    /// <summary>
    ///  Style the given message using ANSI SGR escape sequences so the
    ///  foreground color is changed and terminated by a reset sequence.
    /// </summary>
    public static string GetColorStr(string msg, ConsoleColor color)
    {
        if (msg.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(msg));
        }
        return $"{GetColorSequence(color)}{msg}{RESET}";
    }

    /// <summary>
    ///  Get the ANSI foreground color SGR escape sequence that
    ///  corresponds to the given console color.
    /// </summary>
    private static string GetColorSequence(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black       => $"{ESCAPE}[0;30m",
            ConsoleColor.DarkBlue    => $"{ESCAPE}[0;34m",
            ConsoleColor.DarkGreen   => $"{ESCAPE}[0;32m",
            ConsoleColor.DarkCyan    => $"{ESCAPE}[0;36m",
            ConsoleColor.DarkRed     => $"{ESCAPE}[0;31m",
            ConsoleColor.DarkMagenta => $"{ESCAPE}[0;35m",
            ConsoleColor.DarkYellow  => $"{ESCAPE}[0;33m",
            ConsoleColor.Gray        => $"{ESCAPE}[0;37m",
            ConsoleColor.DarkGray    => $"{ESCAPE}[1;30m",
            ConsoleColor.Blue        => $"{ESCAPE}[1;34m",
            ConsoleColor.Green       => $"{ESCAPE}[1;32m",
            ConsoleColor.Cyan        => $"{ESCAPE}[1;36m",
            ConsoleColor.Red         => $"{ESCAPE}[1;31m",
            ConsoleColor.Magenta     => $"{ESCAPE}[1;35m",
            ConsoleColor.Yellow      => $"{ESCAPE}[1;33m",
            ConsoleColor.White       => $"{ESCAPE}[1;37m",
            _                        => RESET,
        };
    }
}
