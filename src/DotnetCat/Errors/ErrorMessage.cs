using System;
using DotnetCat.Utils;

namespace DotnetCat.Errors
{
    /// <summary>
    ///  Custom error message specifically related to DotnetCat.
    /// </summary>
    internal class ErrorMessage
    {
        /// <summary>
        ///  Initialize the object.
        /// </summary>
        public ErrorMessage(string msg) => Message = msg;

        /// Error message string
        public string Message { get; private set; }

        /// <summary>
        ///  Interpolate the given argument in the underlying message string.
        /// </summary>
        public string Build(string arg)
        {
            bool isBuilt = MsgBuilt();
            bool nullArg = arg.IsNullOrEmpty();

            // Missing required argument
            if (nullArg && !isBuilt)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            // Message already built
            if (!nullArg && isBuilt)
            {
                string errorMsg = "The message has already been built";
                throw new ArgumentException(errorMsg, nameof(arg));
            }

            if (!isBuilt)
            {
                Message = Message.Replace("%", arg).Replace("{}", arg);
            }
            return Message;
        }

        /// <summary>
        ///  Determine whether the underlying message string contains
        ///  any format specifier substrings ('%', '{}').
        /// </summary>
        private bool MsgBuilt()
        {
            return !Message.Contains('%') && !Message.Contains("{}");
        }
    }
}
