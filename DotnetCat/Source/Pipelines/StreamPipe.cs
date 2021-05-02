using System;
using System.IO;
using DotnetCat.Contracts;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline for standard console stream
    /// </summary>
    class StreamPipe : Pipeline, IConnectable
    {
        /// <summary>
        /// Initialize object
        /// </summary>
        public StreamPipe(StreamReader src, StreamWriter dest) : base()
        {
            Source = src ?? throw new ArgumentNullException(nameof(src));
            Dest = dest ?? throw new ArgumentNullException(nameof(dest));
        }
    }
}
