namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Socket pipeline enumeration type
    /// </summary>
    internal enum PipeType : ushort
    {
        Stream,   // Standard console stream pipeline
        File,     // File transfer pipeline
        Process,  // Executable process pipeline
        Status,   // Connection status pipeline
        Text      // User-defined text pipeline
    }
}
