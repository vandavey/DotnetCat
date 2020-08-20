using System;
using System.IO;

namespace DotnetCat.Pipes
{
    /// <summary>
    /// Handle shell process communication operations
    /// </summary>
    class ShellPipe : StreamPipe, IConnectable
    {
        /// Initialize new ShellPipe
        public ShellPipe(StreamReader src, StreamWriter dest) : base()
        {
            this.Source = src ?? throw new ArgumentNullException("src");
            this.Dest = dest ?? throw new ArgumentNullException("dest");
        }
    }
}
