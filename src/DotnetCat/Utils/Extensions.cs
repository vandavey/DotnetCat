using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetCat.Utils;

/// <summary>
///  Utility class for user-defined extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///  Determine whether a string is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty(this string? str)
    {
        return str is null || !str.Trim().Any();
    }

    /// <summary>
    ///  Determine whether a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? iEnum)
    {
        return iEnum is null || !iEnum.Any();
    }

    /// <summary>
    ///  Determine whether a string ends with the given string value.
    /// </summary>
    public static bool EndsWithValue(this string? str, string? value)
    {
        bool endsWith = false;

        if (str is not null && value is not null)
        {
            endsWith = str.EndsWith(value);
        }
        return endsWith;
    }

    /// <summary>
    ///  Determine whether a string starts with the given character.
    /// </summary>
    public static bool StartsWithValue(this string? str, char value)
    {
        return str?.StartsWith(value) ?? false;
    }

    /// <summary>
    ///  Determine whether a string starts with the given string value.
    /// </summary>
    public static bool StartsWithValue(this string? str, string? value)
    {
        bool startsWith = false;

        if (str is not null && value is not null)
        {
            startsWith = str.StartsWith(value);
        }
        return startsWith;
    }

    /// <summary>
    ///  Join each element of a collection separated by the given delimiter.
    /// </summary>
    public static string Join<T>(this IEnumerable<T>? iEnum, string? delim = default)
    {
        if (iEnum.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(iEnum));
        }
        return string.Join(delim, iEnum ?? Array.Empty<T>());
    }

    /// <summary>
    ///  Join each element of a collection separated by the default system EOL.
    /// </summary>
    public static string JoinLines<T>(this IEnumerable<T>? iEnum)
    {
        return Join(iEnum, Environment.NewLine);
    }
}
