using System;
using System.IO;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Handle shell process output streams
    /// </summary>
    class ShellPipe : StreamPipe, ICloseable
    {
        /// Initialize new TextPipe
        public ShellPipe(StreamReader source, StreamWriter dest) : base()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            else if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }

            this.Source = source;
            this.Dest = dest;
            this.Client = Program.SockShell.Client;
        }
    }
}
