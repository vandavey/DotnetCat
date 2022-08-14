using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCat.Utils;

namespace DotnetCat.IO
{
    /// <summary>
    ///  Application console output style utility class.
    /// </summary>
    internal static class Style
    {
        private static readonly List<Status> _statuses;  // Status list

        /// <summary>
        ///  Initialize the static class members.
        /// </summary>
        static Style() => _statuses = new List<Status>
        {
            new Status(ConsoleColor.Cyan, Level.Info),
            new Status(ConsoleColor.Green, Level.Output),
            new Status(ConsoleColor.Red, Level.Error),
            new Status(ConsoleColor.Yellow, Level.Warn)
        };

        /// <summary>
        ///  Write the given error message to the standard error stream.
        /// </summary>
        public static void Error(string msg) => Status(Level.Error, msg);

        /// <summary>
        ///  Write the given completion status message to the standard output stream.
        /// </summary>
        public static void Output(string msg) => Status(Level.Output, msg);

        /// <summary>
        ///  Write the given informational message to the standard output stream.
        /// </summary>
        public static void Info(string msg) => Status(Level.Info, msg);

        /// <summary>
        ///  Write the given warning status message to the standard error stream.
        /// </summary>
        public static void Warn(string msg) => Status(Level.Warn, msg);

        /// <summary>
        ///  Write the given status message to a standard console stream
        ///  based on the given console output level.
        /// <summary>
        private static void Status(Level level, string msg)
        {
            if (msg.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(msg));
            }
            int index = IndexOfStatus(level);

            using TextWriter stream = level switch
            {
                Level.Error or Level.Warn       => Console.Error,
                Level.Info or Level.Output or _ => Console.Out
            };

            Status status = _statuses[index];
            string symbol = Sequence.GetColorStr(status.Symbol, status.Color);

            stream.WriteLine($"{symbol} {msg}");
        }

        /// <summary>
        ///  Get the index of the given output level in the underlying status list.
        /// <summary>
        private static int IndexOfStatus(Level level)
        {
            Status status = _statuses.Where(s => s.Level == level).First();
            return _statuses.IndexOf(status);
        }
    }
}
