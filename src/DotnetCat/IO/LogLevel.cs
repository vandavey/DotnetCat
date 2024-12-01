namespace DotnetCat.IO;

/// <summary>
///  Console logging level enumeration type.
/// </summary>
internal enum LogLevel : byte
{
    /// <summary>
    ///  General information log messages (stdout).
    /// </summary>
    Info,

    /// <summary>
    ///  Status and completion log messages (stdout).
    /// </summary>
    Status,

    /// <summary>
    ///  Warning log messages (stderr).
    /// </summary>
    Warn,

    /// <summary>
    ///  Error and exception log messages (stderr).
    /// </summary>
    Error
}
