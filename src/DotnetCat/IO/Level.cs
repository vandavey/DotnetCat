namespace DotnetCat.IO
{
    /// <summary>
    ///  Console output level enumeration type.
    /// </summary>
    internal enum Level : ushort
    {
        Error,   // Error/exception messages (stderr)
        Info,    // General messages (stdout)
        Output,  // Completion messages (stdout)
        Warn     // Warning messages (stderr)
    }
}
