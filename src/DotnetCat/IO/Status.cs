using System;

namespace DotnetCat.IO
{
    /// <summary>
    ///  Application console status configuration
    /// </summary>
    internal class Status
    {
        /// <summary>
        ///  Initialize object
        /// </summary>
        public Status(ConsoleColor color, Level level)
        {
            Level = level;
            EscSequence = Sequence.GetColorStr(Color = color);

            Symbol = level switch
            {
                Level.Error     => "[x]",
                Level.Output    => "[+]",
                Level.Warn      => "[!]",
                Level.Info or _ => "[*]"
            };
        }

        /// Status symbol color
        public ConsoleColor Color { get; }

        /// Status output level
        public Level Level { get; }

        /// Foreground color sequence
        public string EscSequence { get; }

        /// Status prefix symbol
        public string Symbol { get; }
    }
}
