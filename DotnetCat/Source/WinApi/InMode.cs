using System;
using DWORD = System.UInt32;

namespace DotnetCat.WinApi
{
    /// <summary>
    ///  Window API console input mode flags
    /// </summary>
    [Flags]
    public enum InMode : DWORD
    {
        UNKNOWN_INPUT_MODE            = 0x0000,
        ENABLE_PROCESSED_INPUT        = 0x0001,
        ENABLE_LINE_INPUT             = 0x0002,
        ENABLE_ECHO_INPUT             = 0x0004,
        ENABLE_WINDOW_INPUT           = 0x0008,
        ENABLE_MOUSE_INPUT            = 0x0010,
        ENABLE_INSERT_MODE            = 0x0020,
        ENABLE_QUICK_EDIT_MODE        = 0x0040,
        ENABLE_EXTENDED_FLAGS         = 0x0080,
        ENABLE_AUTO_POSITION          = 0x0100,
        ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200
    }
}
