using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace DotnetCat.Shell;

/// <summary>
///  System information provider utility class.
/// </summary>
internal static class SysInfo
{
    /// <summary>
    ///  Environment-specific newline string.
    /// </summary>
    public static string Eol => Environment.NewLine;

    /// <summary>
    ///  Local machine hostname.
    /// </summary>
    public static string Hostname => Environment.MachineName;

    /// <summary>
    ///  Determine whether the local operating system is Linux.
    /// </summary>
    public static bool IsLinux() => OperatingSystem.IsLinux();

    /// <summary>
    ///  Determine whether the local operating system is Windows.
    /// </summary>
    public static bool IsWindows() => OperatingSystem.IsWindows();

    /// <summary>
    ///  Get a string containing information about the local drives.
    /// </summary>
    public static string AllDriveInfo()
    {
        StringBuilder info = new($"""
            Drive Information
            -----------------{Eol}
            """);

        DriveInfo[] drives = DriveInfo.GetDrives();

        for (int i = 0; i < drives.Length; i++)
        {
            if (i != drives.Length - 1)
            {
                info.AppendLine(SizeInfo(drives[i]) + Eol);
                continue;
            }
            info.Append(SizeInfo(drives[i]));
        }
        return info.ToString();
    }

    /// <summary>
    ///  Get a string containing size information from the given drive information.
    /// </summary>
    private static string SizeInfo(DriveInfo info)
    {
        string infoString = $"""
            Drive Name : {info.Name}
            Drive Type : {info.DriveType}
            Total Size : {ToGigabytes(info.TotalSize):n2} GB
            Used Space : {ToGigabytes(info.TotalSize - info.TotalFreeSpace):n2} GB
            Free Space : {ToGigabytes(info.TotalFreeSpace):n2} GB
            """;
        return infoString;
    }

    /// <summary>
    ///  Convert the given size in bytes to gigabytes.
    /// </summary>
    private static double ToGigabytes<T>(T bytes) where T : INumber<T>
    {
        return Convert.ToDouble(bytes) / 1024.0 / 1024.0 / 1024.0;
    }
}
