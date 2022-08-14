using System;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Network;

namespace DotnetCat.Contracts
{
    /// <summary>
    ///  Interface for enforcing mechanisms to release unmanaged socket
    ///  resources before exiting the application (when an error occurs).
    /// </summary>
    internal interface ISockErrorHandled : IErrorHandled
    {
        /// <summary>
        ///  Dispose of all unmanaged socket resources and handle the given error.
        /// </summary>
        void PipeError(Except type,
                       HostEndPoint target,
                       Exception? ex = default,
                       Level level = default);
    }
}
