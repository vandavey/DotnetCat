using System;

#if WINDOWS
using DotnetCat.Errors;
using DotnetCat.Utils;
#endif // WINDOWS

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows console API interoperability utility class.
/// </summary>
internal static class ConsoleApi
{
#if WINDOWS
    private const int STD_ERROR_HANDLE = -12;
    private const int STD_INPUT_HANDLE = -10;
    private const int STD_OUTPUT_HANDLE = -11;
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
            using WinSafeHandle stdInHandle = GetStdHandle(STD_INPUT_HANDLE);
            using WinSafeHandle stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            using WinSafeHandle stdErrHandle = GetStdHandle(STD_ERROR_HANDLE);

            SetConsoleMode(stdInHandle, GetConsoleMode(stdInHandle, (uint)inMode));
            SetConsoleMode(stdOutHandle, GetConsoleMode(stdOutHandle, (uint)outMode));
            SetConsoleMode(stdErrHandle, GetConsoleMode(stdErrHandle, (uint)outMode));

            VirtualTermEnabled = true;
        }
    }

    /// <summary>
    ///  Get a safe handle to the standard console buffer
    ///  corresponding to the given console buffer ID.
    /// </summary>
    private static WinSafeHandle GetStdHandle(int bufferId)
    {
        if (!bufferId.EqualsAny(STD_INPUT_HANDLE, STD_OUTPUT_HANDLE, STD_ERROR_HANDLE))
        {
            throw new ArgumentException("Invalid console buffer ID.", nameof(bufferId));
        }
        WinSafeHandle safeHandle = new(WinInterop.GetStdHandle(bufferId));

        if (safeHandle.IsInvalid)
        {
            throw new ExternException(nameof(WinInterop.GetStdHandle));
        }
        return safeHandle;
    }

    /// <summary>
    ///  Get a new mode to set for the console buffer
    ///  corresponding to the given console buffer safe handle.
    /// </summary>
    private static uint GetConsoleMode(WinSafeHandle safeHandle, uint mode)
    {
        ThrowIf.InvalidHandle(safeHandle);
        ThrowIf.Zero(mode);

        if (!WinInterop.GetConsoleMode(safeHandle.Handle, out uint outputMode))
        {
            throw new ExternException(nameof(WinInterop.GetConsoleMode));
        }
        return outputMode | mode;
    }

    /// <summary>
    ///  Set the mode of the console buffer corresponding
    ///  to the given console buffer safe handle.
    /// </summary>
    private static void SetConsoleMode(WinSafeHandle safeHandle, uint mode)
    {
        ThrowIf.InvalidHandle(safeHandle);
        ThrowIf.Zero(mode);

        if (!WinInterop.SetConsoleMode(safeHandle.Handle, mode))
        {
            throw new ExternException(nameof(WinInterop.SetConsoleMode));
        }
    }
#endif // WINDOWS
}
