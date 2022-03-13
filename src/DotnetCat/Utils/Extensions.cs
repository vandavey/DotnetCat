using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetCat.Utils
{
    /// <summary>
    ///  Utility class for custom extension methods
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        ///  Determine whether the given string is null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return str is null || (str.Trim().Length == 0);
        }

        /// <summary>
        ///  Determine whether the given string builder is null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this StringBuilder sb)
        {
            return sb is null || sb.ToString().Trim().IsNullOrEmpty();
        }

        /// <summary>
        ///  Determine whether the given array is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array is null || (array.Length == 0);
        }

        /// <summary>
        ///  Determine whether the given list is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list is null || (list.Count == 0);
        }

        /// <summary>
        ///  Join the given array using the specified delimiter
        /// </summary>
        public static string Join<T>(this T[] array, string delim = default)
        {
            if (array.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(array));
            }
            return string.Join(delim ?? string.Empty, array);
        }

        /// <summary>
        ///  Join the given array using the system specific line separator
        /// </summary>
        public static string JoinLines<T>(this T[] array)
        {
            return Join(array, Program.EOL);
        }
    }
}
