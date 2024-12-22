using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.IO;
using SpecialFolder = System.Environment.SpecialFolder;

namespace DotnetCatTests.IO;

/// <summary>
///  Unit tests for utility class <see cref="FileSys"/>.
/// </summary>
[TestClass]
public class FileSysTests
{
#region PropertyTests
    /// <summary>
    ///  Assert that the current user's home directory path is returned.
    /// </summary>
    [TestMethod]
    public void UserProfile_Getter_ReturnsHomePath()
    {
        SpecialFolder folder = SpecialFolder.UserProfile;

        string expected = Environment.GetFolderPath(folder);
        string actual = FileSys.UserProfile;

        Assert.AreEqual(actual, expected, $"Expected home path: '{expected}'");
    }

    /// <summary>
    ///  Assert that the resulting user home directory path exists.
    /// </summary>
    [TestMethod]
    public void UserProfile_Getter_HomePathExists()
    {
        string homePath = FileSys.UserProfile;
        bool actual = Directory.Exists(homePath);

        Assert.IsTrue(actual, $"User home path '{homePath}' does not exist");
    }
#endregion // PropertyTests

#region MethodTests
    /// <summary>
    ///  Assert that a valid (existing) input file or directory
    ///  path on Linux operating systems returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("~")]
    [DataRow(@".\..")]
    [DataRow("/etc")]
    [DataRow(@"\dev\error")]
    [DataRow("/dev/stdout")]
    public void Exists_ValidLinuxPath_ReturnsTrue(string? path)
    {
        if (OperatingSystem.IsLinux())
        {
            bool actual = FileSys.Exists(path);
            Assert.IsTrue(actual, $"Expected path '{path}' to exist");
        }
    }

    /// <summary>
    ///  Assert that a valid (existing) input file or directory
    ///  path on Windows operating systems returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("~")]
    [DataRow(@".\..")]
    [DataRow(@"C:\Users")]
    [DataRow("/Windows/System32/dism.exe")]
    [DataRow(@"C:\Windows/System32\sfc.exe")]
    public void Exists_ValidWindowsPath_ReturnsTrue(string? path)
    {
        if (OperatingSystem.IsWindows())
        {
            bool actual = FileSys.Exists(path);
            Assert.IsTrue(actual, $"Expected path '{path}' to exist");
        }
    }

    /// <summary>
    ///  Assert that an invalid (nonexistent) input file or directory path returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("")]
    [DataRow("  ")]
#if WINDOWS
    [DataRow("/Windows/Desktop")]
    [DataRow(@"C:\Windows\Files\explorer.exe")]
#elif LINUX
    [DataRow("/usr/shared")]
    [DataRow(@"\bin\files\run")]
#endif // WINDOWS
    public void Exists_InvalidPath_ReturnsFalse(string? path)
    {
        bool actual = FileSys.Exists(path);
        Assert.IsFalse(actual, $"Expected path '{path}' to not exist");
    }

    /// <summary>
    ///  Assert that a null input file or directory path returns false.
    /// </summary>
    [TestMethod]
    public void Exists_NullPath_ReturnsFalse()
    {
        string? path = null;
        bool actual = FileSys.Exists(path);

        Assert.IsFalse(actual, "Expected null path to not exist");
    }

    /// <summary>
    ///  Assert that a valid (existing) input file
    ///  path on Linux operating systems returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("/etc/hosts")]
    [DataRow(@"/bin\sh")]
    [DataRow(@"\dev\stdout")]
    public void FileExists_ValidLinuxPath_ReturnsTrue(string? path)
    {
        if (OperatingSystem.IsLinux())
        {
            bool actual = FileSys.FileExists(path);
            Assert.IsTrue(actual, $"Expected file path '{path}' to exist");
        }
    }

    /// <summary>
    ///  Assert that a valid (existing) input file path
    ///  on Windows operating systems returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("/Windows/explorer.exe")]
    [DataRow(@"C:\Windows/System32\sfc.exe")]
    [DataRow(@"\Windows\System32\dism.exe")]
    public void FileExists_ValidWindowsPath_ReturnsTrue(string? path)
    {
        if (OperatingSystem.IsWindows())
        {
            bool actual = FileSys.FileExists(path);
            Assert.IsTrue(actual, $"Expected file path '{path}' to exist");
        }
    }

    /// <summary>
    ///  Assert that an invalid (nonexistent) input file path returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow("~")]
#if WINDOWS
    [DataRow("~/Documents")]
    [DataRow(@"C:\Windows\System32\explorer.exe")]
#elif LINUX
    [DataRow("/dev/output")]
    [DataRow(@"\bin\sbin\sh")]
#endif // WINDOWS
    public void FileExists_InvalidPath_ReturnsFalse(string? path)
    {
        bool actual = FileSys.FileExists(path);
        Assert.IsFalse(actual, $"Expected file path '{path}' to not exist");
    }

    /// <summary>
    ///  Assert that a null input file path returns false.
    /// </summary>
    [TestMethod]
    public void FileExists_NullPath_ReturnsFalse()
    {
        string? path = null;
        bool actual = FileSys.FileExists(path);

        Assert.IsFalse(actual, "Expected null file path to not exist");
    }

    /// <summary>
    ///  Assert that a valid (existing) input directory path returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("~")]
    [DataRow("/")]
    [DataRow(".")]
    [DataRow("..")]
    [DataRow(@"..\..")]
    public void DirectoryExists_ValidPath_ReturnsTrue(string? path)
    {
        bool actual = FileSys.DirectoryExists(path);
        Assert.IsTrue(actual, $"Expected directory path '{path}' to exist");
    }

    /// <summary>
    ///  Assert that an invalid (nonexistent) input directory path returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("")]
    [DataRow("  ")]
#if WINDOWS
    [DataRow("~/Uploads")]
    [DataRow(@"\Windows\System32\explorer.exe")]
#elif LINUX
    [DataRow("/bin/sh")]
    [DataRow(@"\dev\output")]
#endif // WINDOWS
    public void DirectoryExists_InvalidPath_ReturnsFalse(string? path)
    {
        bool actual = FileSys.DirectoryExists(path);
        Assert.IsFalse(actual, $"Expected directory path '{path}' to not exist");
    }

    /// <summary>
    ///  Assert that a null input directory path returns false.
    /// </summary>
    [TestMethod]
    public void DirectoryExists_NullPath_ReturnsFalse()
    {
        string? path = null;
        bool actual = FileSys.DirectoryExists(path);

        Assert.IsFalse(actual, "Expected null directory path to not exist");
    }
#endregion // MethodTests
}
