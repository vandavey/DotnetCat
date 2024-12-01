namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Socket pipeline file transfer option enumeration type.
/// </summary>
internal enum TransferOpt : byte
{
    /// <summary>
    ///  No stream redirection.
    /// </summary>
    None,

    /// <summary>
    ///  Redirect source stream data to file stream.
    /// </summary>
    Collect,

    /// <summary>
    ///  Redirect file stream data to destination stream.
    /// </summary>
    Transmit
}
