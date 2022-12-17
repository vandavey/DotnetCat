using System;

#if WINDOWS
using System.Runtime.InteropServices;
using BOOL = System.Boolean;
using DWORD = System.UInt32;
using HANDLE = System.IntPtr;
#endif // WINDOWS

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows console API interoperability utility class.
/// </summary>
internal static partial class ConsoleApi
{
#if WINDOWS
    private const int NULL = 0;

    private const int STD_INPUT_HANDLE = -10;

    private const int STD_OUTPUT_HANDLE = -11;

    private const nint INVALID_HANDLE_VALUE = -1;
#endif // WINDOWS

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static ConsoleApi()
    {
        VirtualTermEnabled = !OperatingSystem.IsWindows();
        EnableVirtualTerm();
    }

    /// Virtual terminal processing enabled
    public static bool VirtualTermEnabled { get; private set; }

    /// <summary>
    ///  Enable console virtual terminal sequence processing.
    /// </summary>
    public static void EnableVirtualTerm()
    {
    #if WINDOWS
        if (!VirtualTermEnabled)
        {
            InMode inMode = InMode.ENABLE_VIRTUAL_TERMINAL_INPUT;
            OutMode outMode = OutMode.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            EnableVirtualTerm(inMode, outMode);
        }
    #endif // WINDOWS
    }

#if WINDOWS
    /// <summary>
    ///  Enable console virtual terminal sequence processing using the
    ///  given console input and console output modes.
    /// </summary>
    public static void EnableVirtualTerm(InMode inMode, OutMode outMode)
    {
        if (!VirtualTermEnabled)
        {
            HANDLE stdInHandle = GetStdHandle(STD_INPUT_HANDLE);
            HANDLE stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            // Failed to acquire stream handles
            if (!ValidHandle(stdInHandle) || !ValidHandle(stdOutHandle))
            {
                ExternError(nameof(GetStdHandle));
            }

            DWORD stdInMode = GetMode(stdInHandle, GetDWORD(inMode));
            DWORD stdOutMode = GetMode(stdOutHandle, GetDWORD(outMode));

            SetMode(stdInHandle, stdInMode);
            SetMode(stdOutHandle, stdOutMode);

            VirtualTermEnabled = true;
        }
    }

    /// <summary>
    ///  Get a new mode to set for the console buffer that
    ///  corresponds to the given console buffer handle.
    /// </summary>
    private static DWORD GetMode(HANDLE handle, DWORD mode)
    {
        if (!ValidHandle(handle))
        {
            throw new ArgumentException("Invalid handle", nameof(handle));
        }

        if (mode == NULL)
        {
            throw new ArgumentException("No bit flag set", nameof(mode));
        }

        // Failed to get console stream mode
        if (!GetConsoleMode(handle, out DWORD streamMode))
        {
            ExternError(nameof(GetConsoleMode));
        }
        return streamMode |= mode;
    }

    /// <summary>
    ///  Set the mode of the console buffer that corresponds
    ///  to the given console buffer handle.
    /// </summary>
    private static void SetMode(HANDLE handle, DWORD mode)
    {
        if (!ValidHandle(handle))
        {
            throw new ArgumentException("Invalid handle", nameof(handle));
        }

        if (mode == NULL)
        {
            throw new ArgumentException("No bit flag set", nameof(mode));
        }

        // Failed to set console stream mode
        if (!SetConsoleMode(handle, mode))
        {
            ExternError(nameof(SetConsoleMode));
        }
    }

    /// <summary>
    ///  Get the current input mode or output mode of the given
    ///  console input buffer or console output buffer.
    /// </summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial BOOL GetConsoleMode(HANDLE hConsoleHandle,
                                               out DWORD lpMode);

    /// <summary>
    ///  Get the calling thread's most recent Windows error code.
    /// </summary>
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static partial DWORD GetLastError();

    /// <summary>
    ///  Get a handle to the given standard console buffer.
    /// </summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial HANDLE GetStdHandle(int nStdHandle);

    /// <summary>
    ///  Set the input mode or output mode of the given console
    ///  input buffer or console output buffer.
    /// </summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial BOOL SetConsoleMode(HANDLE hConsoleHandle, DWORD dwMode);

    /// <summary>
    ///  Determine whether the given console buffer handle is valid.
    /// </summary>
    private static bool ValidHandle(HANDLE handle)
    {
        return handle != INVALID_HANDLE_VALUE && handle != HANDLE.Zero;
    }

    /// <summary>
    ///  Convert the given console mode enumeration object to a DWORD.
    /// </summary>
    private static DWORD GetDWORD<TEnum>(TEnum mode) where TEnum : Enum
    {
        if (mode is not InMode and not OutMode)
        {
            throw new ArgumentException("Invalid enum type", nameof(mode));
        }
        return Convert.ToUInt32(mode);
    }

    /// <summary>
    ///  Throw an exception for an error that occurred in the external
    ///  function that corresponds to the given extern name.
    /// </summary>
    private static void ExternError(string externName)
    {
        DWORD errorCode = GetLastError();
        throw new ExternalException($"Error in extern {externName}: {errorCode}");
    }
#endif // WINDOWS
}
