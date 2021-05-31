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
        /// <summary>
        /// Release unmanaged resources and handle error
        /// </summary>
        void PipeError(Except type,
                       string arg,
                       Exception ex = default,
                       Level level = default);
    }
}
