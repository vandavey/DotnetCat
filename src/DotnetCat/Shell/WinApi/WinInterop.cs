#if WINDOWS
using System.Runtime.InteropServices;
using static DotnetCat.Shell.WinApi.Constants;

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows API interoperability utility class.
/// </summary>
internal static partial class WinInterop
{
    /// <summary>
    ///  Close the given open object handle.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(nint hObject);

    /// <summary>
    ///  Get the input or output mode of the console buffer
    ///  corresponding to the given console buffer handle.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    /// <summary>
    ///  Set the input or output mode of the console buffer
    ///  corresponding to the given console buffer handle.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    /// <summary>
    ///  Get a handle to the standard console buffer
    ///  corresponding to the given console buffer ID.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysInt)]
    public static partial nint GetStdHandle(int nStdHandle);

    /// <summary>
    ///  Get the error code returned by the last extern call.
    /// </summary>
    public static int GetLastError() => Marshal.GetLastWin32Error();
}

#endif // WINDOWS
