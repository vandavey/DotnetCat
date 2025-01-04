using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DotnetCat.Errors;
using DotnetCat.Shell.WinApi;
using DotnetCat.Utils;

namespace DotnetCat.IO;

/// <summary>
///  Virtual terminal control sequence utility class.
/// </summary>
internal static partial class Sequence
{
    private const string ESC = "\e";
    private const string CLEAR = $"{CSI}H{CSI}2J{CSI}3J";
    private const string CSI = $"{ESC}[";
    private const string RESET = $"{CSI}0m";

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Sequence() => ConsoleApi.EnableVirtualTerm();

    /// <summary>
    ///  Clear the current console screen buffer.
    /// </summary>
    public static void ClearScreen() => Console.Write(CLEAR);

    /// <summary>
    ///  Get the length of the given message excluding ANSI SGR control sequences.
    /// </summary>
    public static int Length([NotNull] string? msg) => Decolorize(msg).Length;

    /// <summary>
    ///  Get the ANSI foreground color SGR control sequence
    ///  that corresponds to the given console color.
    /// </summary>
    public static string Colorize(ConsoleColor color) => ColorSequence(color);

    /// <summary>
    ///  Style the given message using ANSI SGR control sequences so the
    ///  foreground color is changed and terminated by a reset sequence.
    /// </summary>
    public static string Colorize([NotNull] string? msg, ConsoleColor color)
    {
        ThrowIf.NullOrEmpty(msg);
        return ColorSequence(color) + msg + RESET;
    }

    /// <summary>
    ///  Remove styling from given message by erasing all ANSI SGR control sequences.
    /// </summary>
    public static string Decolorize([NotNull] string? msg) => SgrRegex().Erase(msg);

    /// <summary>
    ///  Get the ANSI foreground color SGR control sequence that
    ///  corresponds to the given console color.
    /// </summary>
    private static string ColorSequence(ConsoleColor color) => color switch
    {
        ConsoleColor.Black       => $"{CSI}0;30m",
        ConsoleColor.DarkBlue    => $"{CSI}0;34m",
        ConsoleColor.DarkGreen   => $"{CSI}0;32m",
        ConsoleColor.DarkCyan    => $"{CSI}0;36m",
        ConsoleColor.DarkRed     => $"{CSI}0;31m",
        ConsoleColor.DarkMagenta => $"{CSI}0;35m",
        ConsoleColor.DarkYellow  => $"{CSI}0;33m",
        ConsoleColor.Gray        => $"{CSI}0;37m",
        ConsoleColor.DarkGray    => $"{CSI}1;30m",
        ConsoleColor.Blue        => $"{CSI}1;34m",
        ConsoleColor.Green       => $"{CSI}38;2;166;226;46m",
        ConsoleColor.Cyan        => $"{CSI}38;2;0;255;255m",
        ConsoleColor.Red         => $"{CSI}38;2;246;0;0m",
        ConsoleColor.Magenta     => $"{CSI}1;35m",
        ConsoleColor.Yellow      => $"{CSI}38;2;250;230;39m",
        ConsoleColor.White       => $"{CSI}1;37m",
        _                        => RESET
    };

    /// <summary>
    ///  ANSI SGR control sequence regular expression.
    /// </summary>
    [GeneratedRegex(@"\e\[[0-9]+(;[0-9]+)*m")]
    private static partial Regex SgrRegex();
}
