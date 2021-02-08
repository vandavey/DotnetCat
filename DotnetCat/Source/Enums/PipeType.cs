namespace DotnetCat.Enums
{
    /// <summary>
    /// Socket pipeline enumeration type
    /// </summary>
    enum PipeType : short
    {
        Default,  // Default pipeline type
        File,     // File transfer pipeline
        Process,  // Executable process pipeline
        Text      // User-defined text pipeline
    }
}
