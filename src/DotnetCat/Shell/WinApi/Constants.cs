#if WINDOWS
namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows console API constant definitions.
/// </summary>
internal static class Constants
{
    public const int STD_ERROR_HANDLE = -12;
    public const int STD_INPUT_HANDLE = -10;
    public const int STD_OUTPUT_HANDLE = -11;

    public const string KERNEL32 = "kernel32.dll";
}

#endif // WINDOWS
