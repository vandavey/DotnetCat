namespace DotnetCat.Errors
{
    /// <summary>
    ///  DotnetCat error and exception enumeration type.
    /// </summary>
    internal enum Except : ushort
    {
        Unhandled,          // Unhandled exception occurred
        ArgsCombo,          // Invalid argument combination
        ConnectionLost,     // Socket connection lost error
        ConnectionRefused,  // Socket connection refused error
        ConnectionTimeout,  // Socket connection timeout occurred
        DirectoryPath,      // Invalid directory file path
        EmptyPath,          // Empty directory file path
        ExePath,            // Invalid executable file path
        ExeProcess,         // Executable process error(s)
        FilePath,           // Invalid file path received (general)
        InvalidAddr,        // Invalid IP address or hostname
        InvalidArgs,        // Invalid cmd-line argument(s)
        InvalidPort,        // Invalid network port(s) number
        NamedArgs,          // Invalid named (optional) argument(s)
        RequiredArgs,       // Invalid required (positional) argument(s)
        Payload,            // Invalid string payload argument
        SocketBind,         // Error binding socket to endpoint
        StringEOL,          // Unable to determine string EOL (end quote)
        UnknownArgs         // Unknown/unexpected argument(s)
    }
}
