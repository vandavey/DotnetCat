using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCat.Enums;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Controllers
{
    /// <summary>
    ///  Application output style controller
    /// </summary>
    internal static class Style
    {
        private static readonly List<Status> _statuses;  // Status list

        /// <summary>
        ///  Initialize static members
        /// </summary>
        static Style() => _statuses = new List<Status>
        {
            new Status(ConsoleColor.Cyan, Level.Info),
            new Status(ConsoleColor.Green, Level.Output),
            new Status(ConsoleColor.Red, Level.Error),
            new Status(ConsoleColor.Yellow, Level.Warn)
        };

        /// <summary>
        ///  Write an error message to the standard error stream
        /// </summary>
        public static void Error(string msg) => Status(Level.Error, msg);

        /// <summary>
        ///  Write a completion status to the standard output stream
        /// </summary>
        public static void Output(string msg) => Status(Level.Output, msg);

        /// <summary>
        ///  Write an informational message to the standard output stream
        /// </summary>
        public static void Info(string msg) => Status(Level.Info, msg);

        /// <summary>
        ///  Write a warning status to the standard error stream
        /// </summary>
        public static void Warn(string msg) => Status(Level.Warn, msg);

        /// <summary>
        ///  Write a status message to a standard console stream
        /// <summary>
        private static void Status(Level level, string msg)
        {
            // Status index
            int index = IndexOfStatus(level);

            _ = msg ?? throw new ArgNullException(nameof(msg));

            // Get standard output/error stream
            using TextWriter stream = level switch
            {
                Level.Error or Level.Warn       => Console.Error,
                Level.Info or Level.Output or _ => Console.Out
            };

            Status status = _statuses[index];
            string symbol = Sequence.GetColorStr(status.Symbol, status.Color);

            // Output status message
            stream.WriteLine($"{symbol} {msg}");
        }

        /// <summary>
        ///  Get the index of a status in the status list
        /// <summary>
        private static int IndexOfStatus(Level level)
        {
            Status status = _statuses.Where(s => s.Level == level).First();
            return _statuses.IndexOf(status);
        }
    }
}
