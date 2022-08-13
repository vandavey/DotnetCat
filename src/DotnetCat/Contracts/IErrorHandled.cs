using System;
using DotnetCat.Errors;
using DotnetCat.IO;

namespace DotnetCat.Contracts
{
    /// <summary>
    ///  Interface for enforcing mechanisms to release unmanaged resources
    ///  before exiting the application (when an error occurs).
    /// </summary>
    internal interface IErrorHandled : IConnectable
    {
        /// <summary>
        ///  Dispose of all unmanaged resources and handle the given error.
        /// </summary>
        void PipeError(Except type,
                       string arg,
                       Exception? ex = default,
                       Level level = default);
    }
}
