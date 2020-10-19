using System;
using DotnetCat.Enums;

namespace DotnetCat.Utils
{
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

        public bool Built { get => !Message.Contains("{}"); }

        public string Message { get; private set; }

        /// Format Error with the specified argument
        public void Build(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(nameof(argument));
            }
            else if (Built)
            {
                throw new ArgumentException(
                    "Argument does not require formatting",
                    paramName: nameof(argument)
                );
            }

            Message = Message.Replace("{}", argument);
        }
    }
}
