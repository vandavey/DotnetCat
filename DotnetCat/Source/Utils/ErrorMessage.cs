using System;
using DotnetCat.Enums;

namespace DotnetCat.Utils
{
    /// <summary>
    /// Custom errors specifically related to DotNetCat
    /// </summary>
    class ErrorMessage
    {
        /// <summary>
        /// Initialize object
        /// </summary>
        public ErrorMessage(string msg) => Value = msg;

        /// Error message does not contain '{}'
        public bool Built => !Value.Contains("{}");

        /// Error message
        public string Value { get; private set; }

        /// <summary>
        /// Format error with the specified argument
        /// </summary>
        public string Build(string argument)
        {
            if (argument is null or "")
            {
                // Missing required argument
                if (!Built)
                {
                    throw new ArgumentNullException(nameof(argument));
                }
                return Value;
            }

            // Interpolation not required
            if (Built)
            {
                throw new ArgumentException(null, nameof(argument));
            }
            return Value = Value.Replace("{}", argument);
        }
    }
}
