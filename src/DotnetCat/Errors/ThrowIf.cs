using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DotnetCat.Network;
using DotnetCat.Utils;

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
                                     string? name = null)
    {
        if (!ConsoleApi.ValidHandle(arg))
        {
            throw new ArgumentException($"Invalid handle: {arg}", name);
        }
    }

    /// <summary>
    ///  Throw an exception if the given console mode is invalid.
    /// </summary>
    public static void InvalidMode([NotNull] uint arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = null)
    {
        if (!ConsoleApi.ValidMode(arg))
        {
            throw new ArgumentException("No bit flag mode set", name);
        }
    }
#endif // WINDOWS

    /// <summary>
    ///  Throw an exception if the given port number is invalid.
    /// </summary>
    public static void InvalidPort([NotNull] int arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = null)
    {
        if (!Net.ValidPort(arg))
        {
            throw new ArgumentException($"Invalid port number: {arg}", name);
        }
    }

    /// <summary>
    ///  Throw an exception if the given argument is null.
    /// </summary>
    public static void Null<T>([NotNull] T? arg,
                               [CallerArgumentExpression(nameof(arg))]
                               string? name = null)
    {
        ArgumentNullException.ThrowIfNull(arg, name);
    }

    /// <summary>
    ///  Throw an exception if the given argument is null or empty.
    /// </summary>
    public static void NullOrEmpty<T>([NotNull] IEnumerable<T>? arg,
                                      [CallerArgumentExpression(nameof(arg))]
                                      string? name = null)
    {
        if (arg.IsNullOrEmpty())
        {
            throw new ArgumentNullException(name);
        }
    }

    /// <summary>
    ///  Throw an exception if the given argument is null or empty.
    /// </summary>
    public static void NullOrEmpty([NotNull] string? arg,
                                   [CallerArgumentExpression(nameof(arg))]
                                   string? name = null)
    {
        if (arg.IsNullOrEmpty())
        {
            throw new ArgumentNullException(name);
        }
    }
}
