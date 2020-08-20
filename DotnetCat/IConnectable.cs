using System;

namespace DotnetCat
{
    /// <summary>
    /// Interface to connect and release resources
    /// </summary>
    interface IConnectable : IDisposable
    {
        /// Connect the specified resources
        void Connect();
    }
}
