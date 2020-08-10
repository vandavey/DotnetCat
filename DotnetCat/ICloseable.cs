using System;

namespace DotnetCat
{
    /// <summary>
    /// Interface for connecting/releasing resources
    /// </summary>
    interface ICloseable
    {
        /// Connect the specified resources
        void Connect();

        /// Release the specified resources
        void Close();
    }
}
