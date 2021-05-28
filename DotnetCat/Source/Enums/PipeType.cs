namespace DotnetCat.Enums
{
    /// <summary>
    /// Socket pipeline enumeration type
    /// </summary>
    enum PipeType : short
    {
        Stream,   // Standard console stream pipeline
        File,     // File transfer pipeline
        Process,  // Executable process pipeline
        Text      // User-defined text pipeline
    }
}
