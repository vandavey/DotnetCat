namespace DotnetCat.Enums
{
    /// <summary>
    /// Command-line named argument enumeration type
    /// </summary>
    enum Argument : short
    {
        Alias,  // Conscise argument form (prefix: '-')
        Flag    // Verbose argument form (prefix: '--')
    }
}
