using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;

[assembly: InternalsVisibleTo("DotnetCatTests")]

namespace DotnetCat.Utils
{
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
            return str is null || str.Trim().Length == 0;
        }

        /// <summary>
        ///  Determine whether a string builder is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty(this StringBuilder? sb)
        {
            return sb is null || sb.ToString().Trim().IsNullOrEmpty();
        }

        /// <summary>
        ///  Determine whether an array is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[]? array)
        {
            return array is null || array.Length == 0;
        }

        /// <summary>
        ///  Determine whether a list is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T>? list)
        {
            return list is null || list.Count == 0;
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
        ///  Join each element of an array separated by the given delimiter.
        /// </summary>
        public static string Join<T>(this T[]? array, string? delim = default)
        {
            if (array.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(array));
            }
            return string.Join(delim ?? string.Empty, array ?? Array.Empty<T>());
        }

        /// <summary>
        ///  Join each element of an array separated by the default system EOL.
        /// </summary>
        public static string JoinLines<T>(this T[]? array)
        {
            return Join(array, Environment.NewLine);
        }
    }
}
