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
        public static void Error(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Error, msg, noNewLine);
        }

        /// Write a completion message to standard output
        public static void Output(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Output, msg, noNewLine);
        }

        /// Write an informational message to standard output
        public static void Info(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Info, msg, noNewLine);
        }

        /// Write a warning message to standard error
        public static void Warn(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Warn, msg, noNewLine);
        }

        /// Write a custom status to standard output
        private static void Status(Level level, string msg,
                                                bool noNewLine = false) {
            int index = IndexOfStatus(level);
            _ = msg ?? throw new ArgNullException(nameof(msg));

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
            Console.ResetColor();

            // Write message to standard stream
            if (noNewLine)
            {
                stream.Write(msg);
                return;
            }
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
