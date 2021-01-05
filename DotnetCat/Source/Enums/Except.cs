namespace DotnetCat.Enums
{
    /// <summary>
    /// DotnetCat error/exception enumeration type
    /// </summary>
    enum Except : short
    {
        Unhandled,          // Unhandled exception occurred
        ArgsCombo,          // Invalid argument combination
        ConnectionLost,     // Socket connection lost error
        ConnectionRefused,  // Socket connection refused error
        DirectoryPath,      // Invalid directory file path
        EmptyPath,          // Empty directory file path
        ExecPath,           // Invalid executable file path
        ExecProcess,        // Executable process error(s)
        FilePath,           // Invalid file path received (general)
        InvalidAddr,        // Invalid IP address or host name
        InvalidArgs,        // Invalid cmd-line argument(s)
        InvalidPort,        // Invalid network port(s) number
        NamedArgs,          // Invalid named (optional) argument(s)
        RequiredArgs,       // Invalid required (positional) argument(s)
        SocketBind,         // Error binding socket to endpoint
        UnknownArgs         // Unknown/unexpected argument(s)
    }
}
