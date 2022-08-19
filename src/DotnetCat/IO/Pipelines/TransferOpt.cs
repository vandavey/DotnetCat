namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Stream file transfer option enumeration type.
/// </summary>
internal enum TransferOpt : ushort
{
    None,      // No redirection
    Collect,   // Redirect source stream data to a file stream.
    Transmit   // Redirect file stream data to a destination stream.
}
