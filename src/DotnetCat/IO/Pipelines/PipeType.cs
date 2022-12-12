namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Stream pipeline enumeration type.
/// </summary>
internal enum PipeType : byte
{
    Stream,   // Standard console stream pipeline
    File,     // File transfer pipeline
    Process,  // Executable process pipeline
    Status,   // Connection status pipeline
    Text      // User-defined text pipeline
}
