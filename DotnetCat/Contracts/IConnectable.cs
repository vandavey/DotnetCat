using System;

namespace DotnetCat.Contracts
{
    /// <summary>
    /// Enforce mechanisms to connect/release resources
    /// </summary>
    interface IConnectable : IDisposable
    {
        /// Connect the specified resources
        void Connect();
    }
}
