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
        private static readonly List<Status> _statuses;  // Status list

        /// <summary>
        /// Initialize static members
        /// </summary>
        static StyleHandler() => _statuses = new List<Status>
        {
            new Status(ConsoleColor.Cyan, Level.Info),
            new Status(ConsoleColor.Green, Level.Output),
            new Status(ConsoleColor.Red, Level.Error),
            new Status(ConsoleColor.Yellow, Level.Warn)
        };

        /// <summary>
        /// Write an error message to standard error
        /// </summary>
        public static void Error(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Error, msg, noNewLine);
        }

        /// <summary>
        /// Write a completion message to standard output
        /// </summary>
        public static void Output(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Output, msg, noNewLine);
        }

        /// <summary>
        /// Write an informational message to standard output
        /// </summary>
        public static void Info(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Info, msg, noNewLine);
        }

        /// <summary>
        /// Write a warning message to standard error
        /// </summary>
        public static void Warn(string msg, bool noNewLine = false)
        {
            _ = msg ?? throw new ArgNullException(nameof(msg));
            Status(Level.Warn, msg, noNewLine);
        }

        /// <summary>
        /// Write a custom status to standard output
        /// <summary>
        private static void Status(Level level,
                                   string msg,
                                   bool noNewLine = false) {
            // Status index
            int index = IndexOfStatus(level);

            _ = msg ?? throw new ArgNullException(nameof(msg));

            // Get standard output/error stream
            using TextWriter stream = level switch
            {
                Level.Error or Level.Warn       => Console.Error,
                Level.Info or Level.Output or _ => Console.Out
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

        /// <summary>
        /// Get the index of a status in Statuses
        /// <summary>
        private static int IndexOfStatus(Level level)
        {
            Status status = _statuses.Where(s => s.Level == level).First();
            return _statuses.IndexOf(status);
        }
    }
}
