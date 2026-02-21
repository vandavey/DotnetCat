namespace DotnetCat.Errors;

/// <summary>
///  Error handling constant definitions.
/// </summary>
internal static class Constants
{
    public const string ADDRESS_IN_USE_ERROR = "The endpoint is already in use: %";
    public const string ARGS_COMBO_ERROR = "Invalid argument combination: %";
    public const string CONNECT_ABORTED_ERROR = "Local software aborted connection to %";
    public const string CONNECT_REFUSED_ERROR = "Connection was actively refused by %";
    public const string CONNECT_RESET_ERROR = "Connection was reset by %";
    public const string DIRECTORY_PATH_ERROR = "Unable to locate parent directory: '%'";
    public const string EMPTY_PATH_ERROR = "A value is required for option(s): %";
    public const string EXE_PATH_ERROR = "Unable to locate executable file: '%'";
    public const string EXE_PROCESS_ERROR = "Unable to launch executable process: %";
    public const string FILE_PATH_ERROR = "Unable to locate file: '%'";
    public const string HOST_NOT_FOUND_ERROR = "Unable to resolve hostname: '%'";
    public const string HOST_UNREACHABLE_ERROR = "Unable to reach host %";
    public const string INVALID_ARGS_ERROR = "Unable to validate argument(s): %";
    public const string INVALID_PORT_ERROR = "Invalid port number: %";
    public const string NAMED_ARGS_ERROR = "Missing value for named argument(s): %";
    public const string NETWORK_DOWN_ERROR = "The network is down: %";
    public const string NETWORK_RESET_ERROR = "Connection to % was lost in network reset";
    public const string NETWORK_UNREACHABLE_ERROR = "The network is unreachable: %";
    public const string PAYLOAD_ERROR = "Invalid payload data: %";
    public const string REQUIRED_ARGS_ERROR = "Missing required argument(s): %";
    public const string SOCKET_ERROR_ERROR = "Unspecified socket error occurred: %";
    public const string STRING_EOL_ERROR = "Missing EOL in argument(s): %";
    public const string TIMED_OUT_ERROR = "Socket timeout occurred: %";
    public const string UNHANDLED_ERROR = "Unhandled exception occurred: %";
    public const string UNKNOWN_ARGS_ERROR = "One or more unknown arguments: %";
}
