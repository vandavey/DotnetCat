using System;

namespace DotnetCat.Errors
{
    /// <summary>
    ///  Custom errors specifically related to DotNetCat
    /// </summary>
    internal class ErrorMessage
    {
        /// <summary>
        ///  Initialize object
        /// </summary>
        public ErrorMessage(string msg) => Message = msg;

        /// Error message
        public string Message { get; private set; }

        /// <summary>
        ///  Format error with the specified argument
        /// </summary>
        public string Build(string arg)
        {
            bool isBuilt = MsgBuilt();
            bool nullArg = arg is null or "";

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
        ///  Determine if the message contains format specifier ('%', '{}')
        /// </summary>
        private bool MsgBuilt()
        {
            return !Message.Contains("%") && !Message.Contains("{}");
        }
    }
}
