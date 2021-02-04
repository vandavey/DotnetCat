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
        /// Release unmanaged resources and handle error
        void PipeError(Except type, IPEndPoint ep, Exception ex = null,
                                                   Level level = Level.Error);
    }
}
