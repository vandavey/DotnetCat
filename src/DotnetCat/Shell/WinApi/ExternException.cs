#if WINDOWS
using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.Errors;

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows API unmanaged function exception.
/// </summary>
internal class ExternException : Exception
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ExternException([NotNull] string? externName) : base()
    {
        Name = ThrowIf.NullOrEmpty(externName);
        ErrorCode = WinInterop.GetLastError();
        Message = $"Error occurred in extern '{Name}': {ErrorCode}.";
    }

    /// <summary>
    ///  Windows API error code.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    ///  Error message.
    /// </summary>
    public override string Message { get; }

    /// <summary>
    ///  Windows API function name.
    /// </summary>
    public string Name { get; }
}

#endif // WINDOWS
