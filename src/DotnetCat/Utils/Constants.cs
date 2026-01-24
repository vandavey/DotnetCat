using System;

namespace DotnetCat.Utils;

/// <summary>
///  Application constant definitions.
/// </summary>
internal static class Constants
{
    public const int ERROR_EXIT_CODE = 1;
    public const int NO_ERROR_EXIT_CODE = 0;

    public const StringComparison IGNORE_CASE_CMP = StringComparison.OrdinalIgnoreCase;

    public const string APP_REPO_URL = "https://github.com/vandavey/DotnetCat";
    public const string APP_USAGE = $"Usage: {EXE_NAME} [OPTIONS] {TARGET_ARG}";
#if WINDOWS
    public const string ENV_VAR_PATH = "Path";
    public const string ENV_VAR_PATH_EXT = "PATHEXT";
    public const string EXE_NAME = "dncat.exe";
#elif LINUX // LINUX
    public const string ENV_VAR_PATH = "PATH";
    public const string EXE_NAME = "dncat";
#endif // WINDOWS
    public const string EXEC_FLAG = "--exec";
    public const string HELP_FLAG = "--help";
    public const string LISTEN_FLAG = "--listen";
    public const string OUTPUT_FLAG = "--output";
    public const string PORT_FLAG = "--port";
    public const string SEND_FLAG = "--send";
    public const string TARGET_ARG = "TARGET";
    public const string TEXT_FLAG = "--text";
    public const string VERBOSE_FLAG = "--verbose";
    public const string ZERO_IO_FLAG = "--zero-io";
}
