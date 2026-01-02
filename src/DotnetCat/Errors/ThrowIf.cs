using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Network;
using DotnetCat.Utils;

#if WINDOWS
using System.Runtime.InteropServices;
#endif // WINDOWS

namespace DotnetCat.Errors;

/// <summary>
///  Utility class for exception handling and validation.
/// </summary>
internal static class ThrowIf
{
    /// <summary>
    ///  Throw an exception if the given port number is invalid.
    /// </summary>
    public static int InvalidPort(int arg,
                                  [CallerArgumentExpression(nameof(arg))]
                                  string? name = default)
    {
        if (!Net.ValidPort(arg))
        {
            throw new ArgumentOutOfRangeException($"Invalid port number: {arg}.", name);
        }
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given socket protocol is invalid.
    /// </summary>
    public static ProtocolType InvalidProtocol(ProtocolType arg,
                                               [CallerArgumentExpression(nameof(arg))]
                                               string? name = default)
    {
        if (arg is not ProtocolType.Tcp and not ProtocolType.Udp)
        {
            throw new ArgumentException($"Invalid socket protocol: {arg}.", name);
        }
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given number is less than a specific value.
    /// </summary>
    public static T LessThan<T>(T arg,
                                T value,
                                [CallerArgumentExpression(nameof(arg))]
                                string? name = default)
        where T : INumber<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(arg, value, name);
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given number is less than zero.
    /// </summary>
    public static T Negative<T>(T arg,
                                [CallerArgumentExpression(nameof(arg))]
                                string? name = default)
        where T : INumber<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arg, name);
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given number is zero.
    /// </summary>
    public static T Zero<T>(T arg,
                            [CallerArgumentExpression(nameof(arg))]
                            string? name = default)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfZero(arg, name);
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given argument is undefined.
    /// </summary>
    public static T Undefined<T>(T arg,
                                 [CallerArgumentExpression(nameof(arg))]
                                 string? name = default)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(arg))
        {
            throw new ArgumentOutOfRangeException(name, arg, "Undefined enumerator.");
        }
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given argument is undefined
    ///  or is equal to the default value of its type.
    /// </summary>
    public static T UndefinedOrDefault<T>(T arg,
                                          [CallerArgumentExpression(nameof(arg))]
                                          string? name = default)
        where T : struct, Enum
    {
        return Default(Undefined(arg, name), name);
    }

    /// <summary>
    ///  Throw an exception if the given argument is null or empty.
    /// </summary>
    public static string NullOrEmpty([NotNull] string? arg,
                                     [CallerArgumentExpression(nameof(arg))]
                                     string? name = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arg, name);
        return arg;
    }

#if WINDOWS
    /// <summary>
    ///  Throw an exception if the given safe handle is invalid.
    /// </summary>
    public static SafeHandle InvalidHandle(SafeHandle arg,
                                           [CallerArgumentExpression(nameof(arg))]
                                           string? name = default)
    {
        if (arg.IsInvalid)
        {
            throw new ArgumentException($"Invalid handle: {arg}.", name);
        }
        return arg;
    }
#endif // WINDOWS

    /// <summary>
    ///  Throw an exception if the given IP address is not an IPv4 address.
    /// </summary>
    public static IPAddress NotIPv4Address([NotNull] IPAddress? arg,
                                           [CallerArgumentExpression(nameof(arg))]
                                           string? name = default)
    {
        if (arg?.AddressFamily is not AddressFamily.InterNetwork)
        {
            throw new ArgumentException("IP address family is not IPv4.", name);
        }
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given argument
    ///  is equal to the default value of its type.
    /// </summary>
    public static T? Default<T>(T? arg,
                                [CallerArgumentExpression(nameof(arg))]
                                string? name = default)
    {
        if (arg.IsDefault())
        {
            throw new ArgumentException($"Default value specified: {arg}.", name);
        }
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given argument is null.
    /// </summary>
    public static T Null<T>([NotNull] T? arg,
                            [CallerArgumentExpression(nameof(arg))]
                            string? name = default)
    {
        ArgumentNullException.ThrowIfNull(arg, name);
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given type parameters are different.
    ///  If the comparison type parameter is a tuple, throws an exception
    ///  if none of the tuple member types match.
    /// </summary>
    public static T TypeMismatch<T, TTypes>(T arg,
                                            [CallerArgumentExpression(nameof(arg))]
                                            string? name = default)
        where T : notnull
    {
        if (arg.GetType() != typeof(TTypes))
        {
            if (!typeof(TTypes).IsValueTuple())
            {
                throw new ArgumentException($"Type not equal to {typeof(TTypes)}.", name);
            }

            if (!typeof(TTypes).GetGenericArguments().Any(t => t == arg.GetType()))
            {
                throw new ArgumentException($"Type not found in {typeof(TTypes)}.", name);
            }
        }
        return arg;
    }

    /// <summary>
    ///  Throw an exception if the given argument is null or empty.
    /// </summary>
    public static IEnumerable<T> NullOrEmpty<T>([NotNull] IEnumerable<T>? arg,
                                                [CallerArgumentExpression(nameof(arg))]
                                                string? name = default)
    {
        if (!Null(arg, name).Any())
        {
            throw new ArgumentException("Collection cannot be empty.", name);
        }
        return arg;
    }
}
