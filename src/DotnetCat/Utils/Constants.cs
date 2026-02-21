using System;

namespace DotnetCat.Utils;

/// <summary>
///  Application constant definitions.
/// </summary>
internal static class Constants
{
    public const char EXEC_FLAG_ALIAS = 'e';
    public const char HELP_FLAG_ALIAS = 'h';
    public const char LISTEN_FLAG_ALIAS = 'l';
    public const char OPT_ARG_PREFIX = '-';
    public const char OUTPUT_FLAG_ALIAS = 'o';
    public const char PORT_FLAG_ALIAS = 'p';
    public const char SEND_FLAG_ALIAS = 's';
    public const char TEXT_FLAG_ALIAS = 't';
    public const char VERBOSE_FLAG_ALIAS = 'v';
    public const char ZERO_IO_FLAG_ALIAS = 'z';

    public const int ERROR_EXIT_CODE = 1;
    public const int NO_ERROR_EXIT_CODE = 0;

    public const StringComparison IGNORE_CASE_CMP = StringComparison.OrdinalIgnoreCase;

    public const string ALIAS_PREFIX = "-";
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
    public const string EXEC_FLAG = $"{FLAG_PREFIX}exec";
    public const string FLAG_PREFIX = "--";
    public const string HELP_FLAG = $"{FLAG_PREFIX}help";
    public const string LISTEN_FLAG = $"{FLAG_PREFIX}listen";
    public const string OUTPUT_FLAG = $"{FLAG_PREFIX}output";
    public const string PORT_FLAG = $"{FLAG_PREFIX}port";
    public const string SEND_FLAG = $"{FLAG_PREFIX}send";
    public const string TARGET_ARG = "TARGET";
    public const string TEXT_FLAG = $"{FLAG_PREFIX}text";
    public const string VERBOSE_FLAG = $"{FLAG_PREFIX}verbose";
    public const string ZERO_IO_FLAG = $"{FLAG_PREFIX}zero-io";
}
