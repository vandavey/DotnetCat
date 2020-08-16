using System;

namespace DotnetCat.Utils
{
    enum ErrorType : int
    {
        ArgCombination,
        ArgValidation,
        ConnectionLost,
        ConnectionRefused,
        DirectoryPath,
        EmptyPath,
        FilePath,
        InvalidAddress,
        InvalidPort,
        NamedArg,
        RequiredArg,
        ShellPath,
        ShellProcess,
        SocketBind,
        UnknownArg
    }

    /// <summary>
    /// Custom errors specifically related to DotNetCat
    /// </summary>
    class Error
    {
        /// Initialize new Client
        public Error(ErrorType type, string msg)
        {
            this.TypeName = type;
            this.Message = msg;
        }

        public ErrorType TypeName { get; }

        public bool IsBuilt { get => !Message.Contains("{}"); }

        public string Message { get; private set; }

        /// Format Error with the specified argument
        public void Build(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException("argument");
            }
            else if (IsBuilt)
            {
                throw new ArgumentException("No formatting required");
            }

            Message = Message.Replace("{}", argument);
        }
    }
}
