using System;
using System.IO;
using DotnetCat.Contracts;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline for external process standard stream data
    /// </summary>
    class ProcessPipe : StreamPipe, IConnectable
    {
        /// <summary>
        /// Initialize object
        /// </summary>
        public ProcessPipe(StreamReader src, StreamWriter dest) : base()
        {
            Source = src ?? throw new ArgumentNullException(nameof(src));
            Dest = dest ?? throw new ArgumentNullException(nameof(dest));
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        ~ProcessPipe() => Dispose();
    }
}
