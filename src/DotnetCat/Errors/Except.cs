namespace DotnetCat.Errors;

/// <summary>
///  DotnetCat error and exception enumeration type.
/// </summary>
internal enum Except : byte
{
    /// <summary>
    ///  Unhandled exception occurred.
    /// </summary>
    Unhandled,

    /// <summary>
    ///  Local IP endpoint already in use.
    /// </summary>
    AddressInUse,

    /// <summary>
    ///  Invalid argument combination specified.
    /// </summary>
    ArgsCombo,

    /// <summary>
    ///  Socket connection was aborted by software.
    /// </summary>
    ConnectionAborted,

    /// <summary>
    ///  Socket connection was refused.
    /// </summary>
    ConnectionRefused,

    /// <summary>
    ///  Socket connection was reset.
    /// </summary>
    ConnectionReset,

    /// <summary>
    ///  Invalid directory path specified.
    /// </summary>
    DirectoryPath,

    /// <summary>
    ///  File path missing in named argument.
    /// </summary>
    EmptyPath,

    /// <summary>
    ///  Invalid executable file path specified.
    /// </summary>
    ExePath,

    /// <summary>
    ///  One or more executable process errors occurred.
    /// </summary>
    ExeProcess,

    /// <summary>
    ///  Invalid file path specified.
    /// </summary>
    FilePath,

    /// <summary>
    ///  Invalid IP address or DNS hostname resolution failure.
    /// </summary>
    HostNotFound,

    /// <summary>
    ///  Unable to find route to specified host.
    /// </summary>
    HostUnreachable,

    /// <summary>
    ///  One or more invalid arguments specified.
    /// </summary>
    InvalidArgs,

    /// <summary>
    ///  One or more invalid network port numbers specified.
    /// </summary>
    InvalidPort,

    /// <summary>
    ///  One or more invalid named arguments specified.
    /// </summary>
    NamedArgs,

    /// <summary>
    ///  Local network is down.
    /// </summary>
    NetworkDown,

    /// <summary>
    ///  Connection dropped in network reset.
    /// </summary>
    NetworkReset,

    /// <summary>
    ///  Attempted to communicate with unreachable network.
    /// </summary>
    NetworkUnreachable,

    /// <summary>
    ///  Invalid string payload argument specified.
    /// </summary>
    Payload,

    /// <summary>
    ///  One or more invalid required arguments.
    /// </summary>
    RequiredArgs,

    /// <summary>
    ///  Unspecified socket error occurred.
    /// </summary>
    SocketError,

    /// <summary>
    ///  Missing one or more string EOL characters.
    /// </summary>
    StringEol,

    /// <summary>
    ///  Socket connection timeout occurred.
    /// </summary>
    TimedOut,

    /// <summary>
    ///  One or more unknown or unexpected arguments.
    /// </summary>
    UnknownArgs
}
