using System;

namespace DotnetCat.Contracts
{
    /// <summary>
    /// Enforce mechanisms to connect and dispose of
    /// any unmanaged object resources
    /// </summary>
    interface IConnectable : IDisposable
    {
        /// <summary>
        /// Connect the unmanaged resources
        /// </summary>
        void Connect();
    }
}
