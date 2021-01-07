using System;
using System.Collections.Generic;
using System.Linq;
using DotnetCat.Utils;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Application output style handler
    /// </summary>
    class StyleHandler
    {
        private readonly List<Status> _statuses;

        /// Initialize new object
        public StyleHandler()
        {
            _statuses = new List<Status>
            {
                new Status(ConsoleColor.Cyan, "info", "[*]"),
                new Status(ConsoleColor.Green, "out", "[+]"),
                new Status(ConsoleColor.Yellow, "warn", "[!]")
            };
        }

        /// Print a custom status to standard output
        public void Status(string msg, string level = "info")
        {
            if (!IsValidLevel(level))
            {
                throw new ArgumentException(
                    message: "Level must be 'info', 'out', or 'warning'",
                    paramName: nameof(level)
                );
            }
            int index = IndexOfStatus(level);

            Console.ForegroundColor = _statuses[index].Color;
            Console.Write($"{_statuses[index].Symbol} ");
            Console.ResetColor();
            Console.WriteLine(msg);
        }

        /// Determine if specified status level is valid
        private bool IsValidLevel(string level)
        {
            return IndexOfStatus(level) > -1;
        }

        /// Get the index of a status in Statuses
        private int IndexOfStatus(string level)
        {
            int statusIndex = -1;

            List<int> query = (from stat in _statuses
                               where stat.Level == level.ToLower()
                               select _statuses.IndexOf(stat)).ToList();

            query?.ForEach(index => statusIndex = index);
            return statusIndex;
        }
    }
}
