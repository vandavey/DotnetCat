namespace DotnetCat.Enums
{
    /// <summary>
    /// Console output status level enumeration type
    /// </summary>
    internal enum Level : short
    {
        Error,   // Error/exception messages (stderr)
        Info,    // General messages (stdout)
        Output,  // Completion messages (stdout)
        Warn     // Warning messages (stderr)
    }
}
