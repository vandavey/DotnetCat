using System;

namespace DotnetCat
{
    /// <summary>
    /// Custom errors specifically related to DotNetCat
    /// </summary>
    class Error
    {
        /// Initialize new Client
        public Error(string name, string message)
        {
            this.Name = name;
            this.Message = message;
        }

        public string Name { get; }

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
