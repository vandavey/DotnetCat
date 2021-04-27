using System;
using System.Net;
using DotnetCat.Enums;

namespace DotnetCat.Contracts
{
    /// <summary>
    /// Enforce mechanisms to release unmanaged socket
    /// resources before exiting (when an error occurs)
    /// </summary>
    interface ISockErrorHandled : IErrorHandled
    {
        /// <summary>
        /// Release unmanaged resources and handle error
        /// </summary>
        void PipeError(Except type, IPEndPoint ep, Exception ex = default,
                                                   Level level = default);
    }
}
