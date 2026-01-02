namespace DotnetCat.Utils;

/// <summary>
///  Command-line argument enumeration type.
/// </summary>
internal enum ArgType : byte
{
    /// <summary>
    ///  Remote or local IPv4 address (<c>TARGET</c>).
    /// </summary>
    Target,

    /// <summary>
    ///  Executable file path (<c>-e</c>, <c>--exec</c>).
    /// </summary>
    Exec,

    /// <summary>
    ///  Display help message (<c>-?</c>/<c>-h</c>, <c>--help</c>).
    /// </summary>
    Help,

    /// <summary>
    ///  Listen for incoming connections (<c>-l</c>, <c>--listen</c>).
    /// </summary>
    Listen,

    /// <summary>
    ///  File transfer output file path (<c>-o</c>, <c>--output</c>).
    /// </summary>
    Output,

    /// <summary>
    ///  Connection port number (<c>-p</c>, <c>--port</c>).
    /// </summary>
    Port,

    /// <summary>
    ///  File transfer input file path (<c>-s</c>, <c>--send</c>).
    /// </summary>
    Send,

    /// <summary>
    ///  User-defined payload (<c>-t</c>, <c>--text</c>).
    /// </summary>
    Text,

    /// <summary>
    ///  Enable verbose console output (<c>-v</c>, <c>--verbose</c>).
    /// </summary>
    Verbose,

    /// <summary>
    ///  Test connection status (<c>-z</c>, <c>--zero-io</c>).
    /// </summary>
    ZeroIo
}
