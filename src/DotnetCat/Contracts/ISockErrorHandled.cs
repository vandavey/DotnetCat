using System;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Network;

namespace DotnetCat.Contracts
{
    /// <summary>
    ///  Enforce mechanisms to release unmanaged socket
    ///  resources before exiting (when an error occurs)
    /// </summary>
    internal interface ISockErrorHandled : IErrorHandled
    {
        /// <summary>
        ///  Release unmanaged resources and handle error
        /// </summary>
        void PipeError(Except type,
                       HostEndPoint target,
                       Exception? ex = default,
                       Level level = default);
    }
}
