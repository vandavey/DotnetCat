using System;

namespace DotnetCat.Utils
{
    /// <summary>
    /// Application console status configuration
    /// </summary>
    class Status
    {
        /// Initialize new object
        public Status(ConsoleColor color, string level, string symbol)
        {
            Color = color;
            Level = level;
            Symbol = symbol;
        }

        public ConsoleColor Color { get; }

        public string Level { get; }

        public string Symbol { get; }
    }
}
