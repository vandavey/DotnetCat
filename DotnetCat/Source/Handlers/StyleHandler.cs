using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetCat.Enums;
using DotnetCat.Utils;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Handlers
{
    /// <summary>
    /// Application output style handler
    /// </summary>
    static class StyleHandler
    {
        private static readonly List<Status> _statuses;

        /// Initialize static members
        static StyleHandler()
        {
            _statuses = new List<Status>
            {
                new Status(ConsoleColor.Cyan, Level.Info),
                new Status(ConsoleColor.Green, Level.Output),
                new Status(ConsoleColor.Red, Level.Error),
                new Status(ConsoleColor.Yellow, Level.Warn)
            };
        }

        /// Write an error message to standard error
        public static void Error(string msg)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Error, msg);
        }

        /// Write a completion message to standard output
        public static void Output(string msg)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Output, msg);
        }

        /// Write an informational message to standard output
        public static void Info(string msg)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Info, msg);
        }

        /// Write a warning message to standard error
        public static void Warn(string msg)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Warn, msg);
        }

        /// Write a custom status to standard output
        private static void Status(Level level, string msg)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            int index = IndexOfStatus(level);

            // Get standard output/error stream
            using TextWriter stream = level switch
            {
                Level.Error => Console.Error,
                Level.Info => Console.Out,
                Level.Output => Console.Out,
                Level.Warn => Console.Error,
                _ => Console.Out,
            };

            // Write symbol to standard stream
            Console.ForegroundColor = _statuses[index].Color;
            stream.Write($"{_statuses[index].Symbol} ");

            // Write message to standard stream
            Console.ResetColor();
            stream.WriteLine(msg);
        }

        /// Get the index of a status in Statuses
        private static int IndexOfStatus(Level level)
        {
            Status status = _statuses.Where(s => s.Level == level).First();
            return _statuses.IndexOf(status);
        }
    }
}
