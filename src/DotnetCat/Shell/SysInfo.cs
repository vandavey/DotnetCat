using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace DotnetCat.Shell;

/// <summary>
///  System information provider utility class.
/// </summary>
internal static class SysInfo
{
    /// <summary>
    ///  Local operating system.
    /// </summary>
    public static Platform OS
    {
        get
        {
            Platform os;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = Platform.Win;
            }
            else
            {
                os = Platform.Nix;
            }
            return os;
        }
    }

    /// <summary>
    ///  Local machine hostname.
    /// </summary>
    public static string Hostname => Environment.MachineName;

    /// <summary>
    ///  Get a string containing information about the local drives.
    /// </summary>
    public static string AllDriveInfo()
    {
        string title = "Drive Information";
        string underline = new('-', title.Length);

        StringBuilder infoBuilder = new($"{title}\n{underline}\n");
        DriveInfo[] allDriveInfo = DriveInfo.GetDrives();

        for (int i = 0; i < allDriveInfo.Length; i++)
        {
            if (i == allDriveInfo.Length - 1)
            {
                infoBuilder.Append(SizeInfo(allDriveInfo[i]));
            }
            else
            {
                infoBuilder.AppendLine($"{SizeInfo(allDriveInfo[i])}\n");
            }
        }
        return infoBuilder.ToString();
    }

    /// <summary>
    ///  Get a string containing size information from the given drive information.
    /// </summary>
    private static string SizeInfo(DriveInfo info)
    {
        string infoString = $"""
            Drive Name : {info.Name}
            Drive Type : {info.DriveType}
            Total Size : {ToGigabytes(info.TotalSize):n3} GB
            Used Space : {ToGigabytes(info.TotalSize - info.TotalFreeSpace):n3} GB
            Free Space : {ToGigabytes(info.TotalFreeSpace):n3} GB
            """;
        return infoString;
    }

    /// <summary>
    ///  Convert the given size in bytes to gigabytes.
    /// </summary>
    private static double ToGigabytes<T>(T bytes) where T : INumber<T>
    {
        return Convert.ToDouble(bytes) / 1024 / 1024 / 1024;
    }
}
