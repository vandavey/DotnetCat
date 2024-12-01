using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.Errors;
using DotnetCat.Network;

namespace DotnetCat.IO;

/// <summary>
///  Interface enforcing mechanisms to connect and release unmanaged resources.
/// </summary>
internal interface IConnectable : IDisposable
{
    /// <summary>
    ///  Connect the unmanaged resource(s).
    /// </summary>
    void Connect();

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    void PipeError(Except type, [NotNull] string? arg, Exception? ex = default);

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    void PipeError(Except type, HostEndPoint target, Exception? ex = default);
}
