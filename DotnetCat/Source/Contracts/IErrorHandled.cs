using System;
using DotnetCat.Enums;

namespace DotnetCat.Contracts
{
    /// <summary>
    /// Enforce mechanisms to release unmanaged resources
    /// before exiting (when an error occurs)
    /// </summary>
    interface IErrorHandled : IConnectable
    {
        /// Release unamanged resources and handle error
        void PipeError(Except type, string arg, Exception ex = null);
    }
}
