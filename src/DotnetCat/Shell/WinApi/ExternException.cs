#if WINDOWS
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using DotnetCat.Errors;

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows API unmanaged function exception.
/// </summary>
internal class ExternException : ExternalException
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ExternException([NotNull] string? externName)
        : base(null, WinInterop.GetLastErrorHr())
    {
        Name = ThrowIf.NullOrEmpty(externName);
        WinErrorCode = WinInterop.GetLastError();
    }

    /// <summary>
    ///  Windows API error code.
    /// </summary>
    public int WinErrorCode { get; }

    /// <summary>
    ///  Error message.
    /// </summary>
    public override string Message
    {
        get => $"External error occurred in '{Name}': {WinErrorCode} ({ErrorCode}).";
    }

    /// <summary>
    ///  Windows API function name.
    /// </summary>
    public string Name { get; }
}

#endif // WINDOWS
