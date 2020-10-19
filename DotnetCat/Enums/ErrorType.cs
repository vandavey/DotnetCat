namespace DotnetCat.Enums
{
    /// <summary>
    /// DotnetCat error/exception enumeration type
    /// </summary>
    enum ErrorType : int
    {
        ArgCombination,
        ArgValidation,
        ConnectionLost,
        ConnectionRefused,
        DirectoryPath,
        EmptyPath,
        FilePath,
        InvalidAddr,
        InvalidPort,
        NamedArg,
        RequiredArg,
        ShellPath,
        ShellProcess,
        SocketBind,
        UnknownArg
    }
}
