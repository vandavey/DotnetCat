using System;
using DotnetCat.Enums;

namespace DotnetCat.Utils
{
    /// <summary>
    /// Application console status configuration
    /// </summary>
    class Status
    {
        /// <summary>
        /// Initialize object
        /// </summary>
        public Status(ConsoleColor color, Level level)
        {
            Color = color;
            Level = level;

            Symbol = level switch
            {
                Level.Error  => "[x]",
                Level.Info   => "[*]",
                Level.Output => "[+]",
                Level.Warn   => "[!]",
                _            => "[*]"
            };
        }

        /// Status symbol color
        public ConsoleColor Color { get; }

        /// Status output level
        public Level Level { get; }

        /// Status prefix symbol
        public string Symbol { get; }
    }
}
