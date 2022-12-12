namespace DotnetCat.Errors;

/// <summary>
///  DotnetCat error and exception enumeration type.
/// </summary>
internal enum Except : byte
{
    Unhandled,          // Unhandled exception occurred
    AddressInUse,       // Local endpoint is already in use
    ArgsCombo,          // Invalid argument combination
    ConnectionRefused,  // Socket connection was refused
    ConnectionReset,    // Socket connection was reset
    DirectoryPath,      // Invalid directory file path
    EmptyPath,          // Empty directory file path
    ExePath,            // Invalid executable file path
    ExeProcess,         // Executable process error(s)
    FilePath,           // Invalid file path received (general)
    HostNotFound,       // Invalid IP address or DNS name resolution failure
    InvalidArgs,        // Invalid command-line argument(s)
    InvalidPort,        // Invalid network port(s) number
    NamedArgs,          // Invalid named (optional) argument(s)
    Payload,            // Invalid string payload argument
    RequiredArgs,       // Invalid required (positional) argument(s)
    SocketError,        // Unspecified socket error
    StringEOL,          // Unable to determine string EOL (end quote)
    TimedOut,           // Socket connection timeout occurred
    UnknownArgs         // Unknown/unexpected argument(s)
}
