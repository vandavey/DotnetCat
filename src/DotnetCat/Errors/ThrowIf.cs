using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using DotnetCat.Network;

#if WINDOWS
using DotnetCat.Shell.WinApi;
#endif // WINDOWS

namespace DotnetCat.Errors;

/// <summary>
///  Utility class for exception handling and validation.
/// </summary>
internal class ThrowIf
{
#if WINDOWS
    /// <summary>
    ///  Throw an exception if the given handle is invalid.
    /// </summary>
    public static void InvalidHandle([NotNull] nint arg,
                                     [CallerArgumentExpression(nameof(arg))]
                                     string? name = default)
    {
        if (!ConsoleApi.ValidHandle(arg))
        {
            throw new ArgumentException($"Invalid handle: {arg}.", name);
        }
    }

    /// <summary>
    ///  Throw an exception if the given console mode is invalid.
    /// </summary>
    public static void InvalidMode([NotNull] uint arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = default)
    {
        if (!ConsoleApi.ValidMode(arg))
        {
            throw new ArgumentException("Expected one or more flags to be set.", name);
        }
    }
#endif // WINDOWS

    /// <summary>
    ///  Throw an exception if the given port number is invalid.
    /// </summary>
    public static void InvalidPort([NotNull] int arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = default)
    {
        if (!Net.ValidPort(arg))
        {
            throw new ArgumentOutOfRangeException($"Invalid port number: {arg}.", name);
        }
    }

    /// <summary>
    ///  Throw an exception if the given number is less than zero.
    /// </summary>
    public static void Negative<T>([NotNull] T arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = default)
        where T : INumber<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arg, name);
    }

    /// <summary>
    ///  Throw an exception if the given argument is null.
    /// </summary>
    public static void Null<T>([NotNull] T? arg,
                               [CallerArgumentExpression(nameof(arg))]
                               string? name = default)
    {
        ArgumentNullException.ThrowIfNull(arg, name);
    }

    /// <summary>
    ///  Throw an exception if the given argument is null or empty.
    /// </summary>
    public static void NullOrEmpty<T>([NotNull] IEnumerable<T>? arg,
                                      [CallerArgumentExpression(nameof(arg))]
                                      string? name = default)
    {
        Null(arg, name);

        if (!arg.Any())
        {
            throw new ArgumentException("Collection cannot be empty.", name);
        }
    }

    /// <summary>
    ///  Throw an exception if the given argument is null or empty.
    /// </summary>
    public static void NullOrEmpty([NotNull] string? arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arg, name);
    }
}
