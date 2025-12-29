namespace DotnetCat.IO;

/// <summary>
///  I/O constant definitions.
/// </summary>
internal static class Constants
{
    public const int READ_BUFFER_SIZE = 1024;
    public const int WRITE_BUFFER_SIZE = READ_BUFFER_SIZE * 4;

    public const string CLEAR = $"{CSI}H{CSI}2J{CSI}3J";
    public const string CSI = $"{ESC}[";
    public const string ERROR_LOG_PREFIX = "[x]";
    public const string ESC = "\e";
    public const string INFO_LOG_PREFIX = "[*]";
    public const string RESET = $"{CSI}0m";
    public const string STATUS_LOG_PREFIX = "[+]";
    public const string WARNING_LOG_PREFIX = "[!]";
}
