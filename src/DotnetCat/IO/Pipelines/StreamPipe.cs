using System;
using System.IO;
using DotnetCat.Contracts;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional stream pipeline used to transfer standard console stream data.
/// </summary>
internal class StreamPipe : Pipeline, IConnectable
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StreamPipe(StreamReader? src, StreamWriter? dest) : base()
    {
        Source = src ?? throw new ArgumentNullException(nameof(src));
        Dest = dest ?? throw new ArgumentNullException(nameof(dest));
    }
}
