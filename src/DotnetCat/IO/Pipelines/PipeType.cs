namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Socket pipeline enumeration type.
/// </summary>
internal enum PipeType : byte
{
    /// <summary>
    ///  Standard console stream socket pipeline.
    /// </summary>
    Stream,

    /// <summary>
    ///  File transfer socket pipeline.
    /// </summary>
    File,

    /// <summary>
    ///  Executable process socket pipeline.
    /// </summary>
    Process,

    /// <summary>
    ///  Connection status socket pipeline.
    /// </summary>
    Status,

    /// <summary>
    ///  Text transmission socket pipeline.
    /// </summary>
    Text
}
