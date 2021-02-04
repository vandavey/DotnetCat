namespace DotnetCat.Enums
{
    /// <summary>
    /// File and directory path enumeration type
    /// </summary>
    enum FileType : short
    {
        None,       // Unknown or unspecified path
        Archive,    // Archive file path
        Device,     // Volume or drive path
        Directory,  // Directory file path
        File,       // Standard file path
        Protected,  // Inaccessible or protected path
    }
}
