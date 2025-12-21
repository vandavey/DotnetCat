using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using DotnetCat.Errors;
using DotnetCat.Shell;

namespace DotnetCat.Utils;

/// <summary>
///  Utility class for user-defined extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///  Add the result of the given functor to a collection.
    /// </summary>
    public static void Add<T>([NotNull] this ICollection<T>? values, Func<T> func)
    {
        ThrowIf.Null(values).Add(func());
    }

    /// <summary>
    ///  Add the results of the given functor to a collection.
    /// </summary>
    public static void AddRange<T>([NotNull] this List<T>? values,
                                   Func<IEnumerable<T>> func)
    {
        ThrowIf.Null(values).AddRange(func());
    }

    /// <summary>
    ///  Perform an action on each element of a collection.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T>? values, Action<T> action)
    {
        values?.ToList().ForEach(action);
    }

    /// <summary>
    ///  Determine whether a string ends with a single or double quotation mark character.
    /// </summary>
    public static bool EndsWithQuote([NotNullWhen(true)] this string? str)
    {
        return str.EndsWithValue('"') || str.EndsWithValue('\'');
    }

    /// <summary>
    ///  Determine whether a string ends with the given character.
    /// </summary>
    public static bool EndsWithValue([NotNullWhen(true)] this string? str, char value)
    {
        return str?.EndsWith(value) ?? false;
    }

    /// <summary>
    ///  Determine whether a string ends with the given substring.
    /// </summary>
    public static bool EndsWithValue([NotNullWhen(true)] this string? str, string? value)
    {
        bool endsWith = false;

        if (str is not null && value is not null)
        {
            endsWith = str.EndsWith(value);
        }
        return endsWith;
    }

    /// <summary>
    ///  Determine whether an object is equal to one or more values in a collection.
    /// </summary>
    public static bool EqualsAny<T>([NotNullWhen(true)] this T? obj,
                                    params IEnumerable<T> values)
    {
        return values.Any(v => RuntimeHelpers.Equals(v, obj));
    }

    /// <summary>
    ///  Determine whether a string is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return str is null || str.Trim().Length == 0;
    }

    /// <summary>
    ///  Determine whether a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? values)
    {
        return values is null || !values.Any();
    }

    /// <summary>
    ///  Determine whether a string is equal to another
    ///  string when all string casing is ignored.
    /// </summary>
    public static bool NoCaseEquals(this string? str, string? value)
    {
        return str?.ToLower() == value?.ToLower();
    }

    /// <summary>
    ///  Determine whether a string starts with a
    ///  single or double quotation mark character.
    /// </summary>
    public static bool StartsWithQuote([NotNullWhen(true)] this string? str)
    {
        return str.StartsWithValue('"') || str.StartsWithValue('\'');
    }

    /// <summary>
    ///  Determine whether a string starts with the given character.
    /// </summary>
    public static bool StartsWithValue([NotNullWhen(true)] this string? str, char value)
    {
        return str?.StartsWith(value) ?? false;
    }

    /// <summary>
    ///  Determine whether a string starts with the given substring.
    /// </summary>
    public static bool StartsWithValue([NotNullWhen(true)] this string? str,
                                       string? value)
    {
        bool startsWith = false;

        if (str is not null && value is not null)
        {
            startsWith = str.StartsWith(value);
        }
        return startsWith;
    }

    /// <summary>
    ///  Erase all substrings matching a regular expression from the given data.
    /// </summary>
    public static string Erase(this Regex regex, [NotNull] string? data)
    {
        return regex.Replace(ThrowIf.NullOrEmpty(data), string.Empty);
    }

    /// <summary>
    ///  Join each element of a collection separated by the given delimiter.
    /// </summary>
    public static string Join<T>([NotNull] this IEnumerable<T>? values,
                                 string? delim = default)
    {
        return string.Join(delim, ThrowIf.NullOrEmpty(values));
    }

    /// <summary>
    ///  Join each element of a collection separated by the default system EOL.
    /// </summary>
    public static string JoinLines<T>([NotNull] this IEnumerable<T>? values)
    {
        return Join(values, SysInfo.Eol);
    }

    /// <summary>
    ///  Normalize all the newline substrings in a string builder.
    /// </summary>
    public static StringBuilder ReplaceLineEndings(this StringBuilder sb)
    {
        return new StringBuilder(sb.ToString().ReplaceLineEndings());
    }

    /// <summary>
    ///  Get the first value of the given type in a collection, or the
    ///  default value of the type if no matching values are found.
    /// </summary>
    public static T? FirstOrDefaultOfType<T>(this IEnumerable? values)
    {
        return values is null ? default : values.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    ///  Enumerate the values of a collection as a collection of
    ///  tuples containing the index and value of each element.
    /// </summary>
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T>? values)
    {
        return values?.Select((v, i) => (i, v)) ?? [];
    }

    /// <summary>
    ///  Enumerate the values of a collection as a collection of tuples
    ///  containing the index and value of each element, then filter the results.
    /// </summary>
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T>? values,
                                                     Func<(int, T), bool> filter)
    {
        return Enumerate(values).Where(filter);
    }
}
