using System;
using System.IO;
using DotnetCat.Contracts;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline for command-shell related data
    /// </summary>
    class ShellPipe : StreamPipe, IConnectable
    {
        /// Initialize new ShellPipe
        public ShellPipe(StreamReader src, StreamWriter dest) : base()
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }
            else if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            this.Source = src;
            this.Dest = dest;
        }
    }
}
