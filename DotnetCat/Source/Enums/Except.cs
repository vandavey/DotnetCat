namespace DotnetCat.Enums
{
    /// <summary>
    /// DotnetCat error and exception enumeration type
    /// </summary>
    enum Except : short
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
