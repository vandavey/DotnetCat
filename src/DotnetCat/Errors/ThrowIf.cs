using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Network;

#if WINDOWS
using System.Runtime.InteropServices;
#endif // WINDOWS

namespace DotnetCat.Errors;

/// <summary>
///  Utility class for exception handling and validation.
/// </summary>
internal class ThrowIf
{
#if WINDOWS
    /// <summary>
    ///  Throw an exception if the given safe handle is invalid.
    /// </summary>
    public static void InvalidHandle([NotNull] SafeHandle arg,
                                     [CallerArgumentExpression(nameof(arg))]
                                     string? name = default)
    {
        if (arg.IsInvalid)
        {
            throw new ArgumentException($"Invalid handle: {arg}.", name);
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
    ///  Throw an exception if the given IP address is not an IPv4 address.
    /// </summary>
    public static void NotIPv4Address([NotNull] IPAddress? arg,
                                      [CallerArgumentExpression(nameof(arg))]
                                      string? name = default)
    {
        if (arg?.AddressFamily is not AddressFamily.InterNetwork)
        {
            throw new ArgumentException("IP address family is not IPv4.", name);
        }
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

    /// <summary>
    ///  Throw an exception if the given number is zero.
    /// </summary>
    public static void Zero<T>([NotNull] T arg,
                               [CallerArgumentExpression(nameof(arg))]
                               string? name = default)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfZero(arg, name);
    }
}
