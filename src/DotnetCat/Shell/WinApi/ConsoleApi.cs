using System;
using System.Runtime.InteropServices;
using BOOL = System.Boolean;
using DWORD = System.UInt32;
using HANDLE = System.IntPtr;

namespace DotnetCat.Shell.WinApi
{
    /// <summary>
    ///  Utility class providing Windows console API interoperability
    /// </summary>
    internal static class ConsoleApi
    {
        private const int NULL = 0;

        private const int STD_INPUT_HANDLE = -10;

        private const int STD_OUTPUT_HANDLE = -11;

        private const nint INVALID_HANDLE_VALUE = -1;

        /// <summary>
        ///  Initialize static members
        /// </summary>
        static ConsoleApi()
        {
            VirtualTermEnabled = !IsWindows();
            EnableVirtualTerm();
        }

        /// Virtual terminal sequences enabled
        public static bool VirtualTermEnabled { get; private set; }

        /// <summary>
        ///  Enable console virtual terminal sequence processing
        /// </summary>
        public static void EnableVirtualTerm()
        {
            if (!VirtualTermEnabled && IsWindows())
            {
                InMode inMode = InMode.ENABLE_VIRTUAL_TERMINAL_INPUT;
                OutMode outMode = OutMode.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

                EnableVirtualTerm(inMode, outMode);
            }
        }

        /// <summary>
        ///  Enable console virtual terminal sequence processing
        /// </summary>
        public static void EnableVirtualTerm(InMode inMode, OutMode outMode)
        {
            if (!VirtualTermEnabled && IsWindows())
            {
                HANDLE stdInHandle = GetStdHandle(STD_INPUT_HANDLE);
                HANDLE stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

                // Failed to acquire stream handles
                if (!ValidHandle(stdInHandle) || !ValidHandle(stdOutHandle))
                {
                    ExternError(nameof(GetStdHandle));
                }

                DWORD stdInMode = GetMode(stdInHandle, GetWord(inMode));
                DWORD stdOutMode = GetMode(stdOutHandle, GetWord(outMode));

                SetMode(stdInHandle, stdInMode);
                SetMode(stdOutHandle, stdOutMode);

                VirtualTermEnabled = true;
            }
        }

        /// <summary>
        ///  Get new mode to set for a standard console stream buffer
        /// </summary>
        private static DWORD GetMode(HANDLE handle, DWORD mode)
        {
            if (!IsWindows())
            {
                throw new PlatformNotSupportedException(nameof(GetMode));
            }

            // Invalid stream handle
            if (!ValidHandle(handle))
            {
                throw new ArgumentException("Invalid handle", nameof(handle));
            }

            // Invalid console mode
            if (mode == NULL)
            {
                throw new ArgumentNullException(nameof(mode), "No bit flag set");
            }

            // Failed to get console stream mode
            if (!GetConsoleMode(handle, out DWORD streamMode))
            {
                ExternError(nameof(GetConsoleMode));
            }
            return streamMode |= mode;
        }

        /// <summary>
        ///  Set new mode for a standard console stream
        /// </summary>
        private static void SetMode(HANDLE handle, DWORD mode)
        {
            if (!IsWindows())
            {
                throw new PlatformNotSupportedException(nameof(SetMode));
            }

            if (!ValidHandle(handle))
            {
                throw new ArgumentException("Invalid handle", nameof(handle));
            }

            if (mode == NULL)
            {
                throw new ArgumentNullException(nameof(mode), "No bit flag set");
            }

            // Failed to set console stream mode
            if (!SetConsoleMode(handle, mode))
            {
                ExternError(nameof(SetConsoleMode));
            }
        }

        /// <summary>
        ///  Get new mode to set for a standard console stream buffer
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern BOOL GetConsoleMode(HANDLE hConsoleHandle,
                                                  out DWORD lpMode);

        /// <summary>
        ///  Get the most recent Windows console API error code
        /// </summary>
        [DllImport("kernel32.dll")]
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
        private static extern BOOL SetConsoleMode(HANDLE hConsoleHandle,
                                                  DWORD dwMode);

        /// <summary>
        ///  Determine if the operating system is Windows
        /// </summary>
        private static BOOL IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        ///  Determine if a standard console stream handle is valid
        /// </summary>
        private static BOOL ValidHandle(HANDLE handle)
        {
            bool invalidHandle = handle == INVALID_HANDLE_VALUE;
            return !invalidHandle || (handle != HANDLE.Zero);
        }

        /// <summary>
        ///  Convert the given mode enumeration object to a DWORD
        /// </summary>
        private static DWORD GetWord<TEnum>(TEnum mode) where TEnum : Enum
        {
            if (mode is not InMode and not OutMode)
            {
                throw new ArgumentException("Invalid enum type", nameof(mode));
            }
            return Convert.ToUInt32(mode);
        }

        /// <summary>
        ///  Throw an exception with the last Windows Console API error code
        /// </summary>
        private static void ExternError(string externName)
        {
            DWORD error = GetLastError();
            Console.Error.WriteLine($"Error in extern {externName}: {error}");

            throw new ExternalException(externName);
        }
    }
}
