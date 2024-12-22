#if WINDOWS
using Microsoft.Win32.SafeHandles;

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows API safe handle.
/// </summary>
internal class WinSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private readonly bool _ownsHandle;  // Underlying handle owned

    private bool _disposed;             // Object disposed

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public WinSafeHandle(nint handle, bool ownsHandle = false) : base(ownsHandle)
    {
        _ownsHandle = ownsHandle;
        _disposed = false;

        Handle = handle;
    }

    /// <summary>
    ///  Windows API handle.
    /// </summary>
    public nint Handle
    {
        get => handle;
        private set => handle = value;
    }

    /// <summary>
    ///  Free the underlying resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_ownsHandle)
            {
                WinInterop.CloseHandle(Handle);
                Handle = nint.Zero;
            }
            _disposed = true;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///  Free the underlying open object handle.
    /// </summary>
    protected override bool ReleaseHandle()
    {
        return !_ownsHandle || WinInterop.CloseHandle(Handle);
    }
}

#endif // WINDOWS
