using System;
using DotnetCat.Enums;

namespace DotnetCat.Utils
{
    /// <summary>
    /// Custom errors specifically related to DotNetCat
    /// </summary>
    class Error
    {
        /// Initialize object
        public Error(Except type, string msg)
        {
            TypeName = type;
            Message = msg;
        }

        public Except TypeName { get; }

        public bool Built => !Message.Contains("{}");

        public string Message { get; private set; }

        /// Format Error with the specified argument
        public void Build(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                if (!Built)
                {
                    throw new ArgumentNullException(nameof(argument));
                }
                return;
            }

            if (Built)
            {
                throw new ArgumentException("Invalid interpolation attempt",
                                            nameof(argument));
            }
            Message = Message.Replace("{}", argument);
        }
    }
}
