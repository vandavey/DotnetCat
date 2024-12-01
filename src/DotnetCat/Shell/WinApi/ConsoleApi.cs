using System;

#if WINDOWS
using System.Runtime.InteropServices;
using DotnetCat.Errors;
using BOOL = bool;
using DWORD = uint;
using HANDLE = nint;
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

    private const string KERNEL32 = "kernel32.dll";
#endif // WINDOWS

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static ConsoleApi()
    {
        VirtualTermEnabled = !OperatingSystem.IsWindows();
        EnableVirtualTerm();
    }

    /// <summary>
    ///  Virtual terminal processing is enabled.
    /// </summary>
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
    ///  Enable console virtual terminal sequence processing
    ///  using the given console input and console output modes.
    /// </summary>
    public static void EnableVirtualTerm(InMode inMode, OutMode outMode)
    {
        if (!VirtualTermEnabled)
        {
            nint stdInHandle = GetStdHandle(STD_INPUT_HANDLE);
            nint stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (!ValidHandle(stdInHandle) || !ValidHandle(stdOutHandle))
            {
                ExternError(nameof(GetStdHandle));
            }

            uint stdInMode = GetMode(stdInHandle, GetDWord(inMode));
            uint stdOutMode = GetMode(stdOutHandle, GetDWord(outMode));

            SetMode(stdInHandle, stdInMode);
            SetMode(stdOutHandle, stdOutMode);

            VirtualTermEnabled = true;
        }
    }

    /// <summary>
    ///  Determine whether the given console buffer handle is valid.
    /// </summary>
    public static bool ValidHandle(nint handle)
    {
        return handle is not NULL and not INVALID_HANDLE_VALUE;
    }

    /// <summary>
    ///  Determine whether the given console mode is valid.
    /// </summary>
    public static bool ValidMode(uint mode) => mode is not NULL;

    /// <summary>
    ///  Get the current input mode or output mode of the
    ///  given console input buffer or console output buffer.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial BOOL GetConsoleMode(HANDLE hConsoleHandle, out DWORD lpMode);

    /// <summary>
    ///  Get a handle to the given standard console buffer.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial HANDLE GetStdHandle(int nStdHandle);

    /// <summary>
    ///  Set the input mode or output mode of the given
    ///  console input buffer or console output buffer.
    /// </summary>
    [LibraryImport(KERNEL32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial BOOL SetConsoleMode(HANDLE hConsoleHandle, DWORD dwMode);

    /// <summary>
    ///  Throw an exception for an error that occurred in the unmanaged
    ///  function that corresponds to the given extern function name.
    /// </summary>
    private static void ExternError(string externName)
    {
        throw new ExternalException($"Error in extern '{externName}'.", GetLastError());
    }

    /// <summary>
    ///  Get the error code returned by the last unmanaged function call.
    /// </summary>
    private static int GetLastError() => Marshal.GetLastWin32Error();

    /// <summary>
    ///  Get a new mode to set for the console buffer that
    ///  corresponds to the given console buffer handle.
    /// </summary>
    private static uint GetMode(nint handle, uint mode)
    {
        ThrowIf.InvalidHandle(handle);
        ThrowIf.InvalidMode(mode);

        if (!GetConsoleMode(handle, out uint streamMode))
        {
            ExternError(nameof(GetConsoleMode));
        }
        return streamMode |= mode;
    }

    /// <summary>
    ///  Set the mode of the console buffer that
    ///  corresponds to the given console buffer handle.
    /// </summary>
    private static void SetMode(nint handle, uint mode)
    {
        ThrowIf.InvalidHandle(handle);
        ThrowIf.InvalidMode(mode);

        if (!SetConsoleMode(handle, mode))
        {
            ExternError(nameof(SetConsoleMode));
        }
    }

    /// <summary>
    ///  Convert the given console mode enumeration type to a <c>DWORD</c>.
    /// </summary>
    private static uint GetDWord<TEnum>(TEnum mode) where TEnum : Enum
    {
        if (Enum.GetUnderlyingType(typeof(TEnum)) != typeof(uint))
        {
            throw new ArgumentException("Invalid underlying type.", nameof(mode));
        }
        return Convert.ToUInt32(mode);
    }
#endif // WINDOWS
}
