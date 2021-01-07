using System;
using DotnetCat.Enums;

namespace DotnetCat.Utils
{
    /// <summary>
    /// Application console status configuration
    /// </summary>
    class Status
    {
        /// Initialize new object
        public Status(ConsoleColor color, Level level)
        {
            Color = color;
            Level = level;

            Symbol = level switch
            {
                Level.Error => "[x]",
                Level.Info => "[*]",
                Level.Output => "[+]",
                Level.Warn => "[!]",
                _ => "[*]"
            };
        }

        public ConsoleColor Color { get; }

        public Level Level { get; }

        public string Symbol { get; }
    }
}
