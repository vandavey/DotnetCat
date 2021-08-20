using System;
using System.Runtime.InteropServices;
using DotnetCat.Enums;
using BOOL = System.Boolean;
using DWORD = System.UInt32;
using HANDLE = System.IntPtr;

namespace DotnetCat.WinApi
{
    /// <summary>
    ///  Application programming interface for Windows console API
    /// </summary>
    internal static class ConsoleApi
    {
        private const int STD_INPUT_HANDLE = -10;

        private const int STD_OUTPUT_HANDLE = -11;

        private const nint INVALID_HANDLE_VALUE = -1;

        private static bool _enabled;  // Virtual terminal enabled

        /// <summary>
        ///  Initialize static members
        /// </summary>
        static ConsoleApi()
        {
            _enabled = OS is Platform.Nix;
            EnableVirtualTerm();
        }

        /// Operating system
        public static Platform OS => Program.OS;

        /// <summary>
        ///  Enable console virtual terminal sequence processing
        /// </summary>
        public static void EnableVirtualTerm()
        {
            if (!_enabled && OS is Platform.Win)
            {
                InMode inMode = InMode.ENABLE_VIRTUAL_TERMINAL_INPUT;
                OutMode outMode = OutMode.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

                EnableVirtualTerm(inMode, outMode);
            }
        }

        /// <summary>
        ///  Enable console virtual terminal sequence processing
        /// </summary>
        public static void EnableVirtualTerm(InMode inputMode,
                                             OutMode outputMode) {
            // Only needed on Windows
            if (!_enabled && OS is Platform.Win)
            {
                HANDLE stdInHandle = GetStdHandle(STD_INPUT_HANDLE);
                HANDLE stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

                // Failed to acquire stream handles
                if (!ValidHandle(stdInHandle) || !ValidHandle(stdOutHandle))
                {
                    ExternError(nameof(GetStdHandle));
                    return;
                }

                DWORD stdInMode = GetMode(stdInHandle, (DWORD)inputMode);
                DWORD stdOutMode = GetMode(stdOutHandle, (DWORD)outputMode);

                // Failed to get console mode
                if (!ValidHandle(stdInHandle) || !ValidHandle(stdOutHandle))
                {
                    ExternError(nameof(GetConsoleMode));
                }

                bool stdInModeSet = SetMode(stdInHandle, stdInMode);
                bool stdOutModeSet = SetMode(stdOutHandle, stdOutMode);

                // Failed to set console mode
                if (!stdInModeSet || !stdOutModeSet)
                {
                    ExternError(nameof(SetConsoleMode));
                }

                _enabled = true;
            }
        }

        /// <summary>
        ///  Get new mode to set for a standard console stream buffer
        /// </summary>
        public static DWORD GetMode(HANDLE handle, DWORD mode)
        {
            if (OS is Platform.Nix)
            {
                throw new PlatformNotSupportedException(nameof(GetMode));
            }

            if (handle == HANDLE.Zero)
            {
                throw new ArgumentException("Invalid handle", nameof(handle));
            }

            if (mode == 0)
            {
                throw new ArgumentException("No bit flag set", nameof(mode));
            }

            // Error getting console stream handle
            if (!GetConsoleMode(handle, out DWORD streamMode))
            {
                streamMode = 0;
            }

            return (streamMode == 0) ? streamMode : (streamMode |= mode);
        }

        /// <summary>
        ///  Set new mode for a standard console stream
        /// </summary>
        public static BOOL SetMode(HANDLE handle, DWORD mode)
        {
            if (OS is Platform.Nix)
            {
                throw new PlatformNotSupportedException(nameof(SetMode));
            }

            if ((handle == HANDLE.Zero) || (handle == INVALID_HANDLE_VALUE))
            {
                throw new ArgumentException("Invalid handle", nameof(handle));
            }

            if (mode == (DWORD)InMode.UNKNOWN_INPUT_MODE)
            {
                throw new ArgumentException("No bit flag set", nameof(mode));
            }

            bool modeSet = SetConsoleMode(handle, mode);

            // Error setting console stream mode
            if (!modeSet)
            {
                ExternError(nameof(SetConsoleMode));
            }
            return modeSet;
        }

        /// <summary>
        ///  Get new mode to set for a standard console stream buffer
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern BOOL GetConsoleMode(
            HANDLE hConsoleHandle,
            [param: MarshalAs(UnmanagedType.U4)]out DWORD lpMode
        );

        /// <summary>
        ///  Get the most recent Windows console API error code
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern DWORD GetLastError();

        /// <summary>
        ///  Get a handle to a standard console stream
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private static extern HANDLE GetStdHandle(int nStdHandle);

        /// <summary>
        ///  Set new mode for a standard console stream
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern BOOL SetConsoleMode(
            HANDLE hConsoleHandle,
            [param: MarshalAs(UnmanagedType.U4)]DWORD dwMode
        );

        /// <summary>
        ///  Determine if a standard console stream handle is valid
        /// </summary>
        private static bool ValidHandle(HANDLE handle)
        {
            bool invalidHandle = handle == INVALID_HANDLE_VALUE;
            return !invalidHandle || (handle != HANDLE.Zero);
        }

        /// <summary>
        ///  Display the last Windows console API error code
        /// </summary>
        private static void ExternError(string externName)
        {
            DWORD error = GetLastError();
            Console.Error.WriteLine($"Error in extern {externName}: {error}");
        }
    }
}
