using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.IO;
using static DotnetCat.IO.Constants;
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
    ///  Assert that the home directory path of the current user is returned.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.UserProfile"/>.
    /// </remarks>
    [TestMethod]
    public void UserProfile_Get_ReturnsHomePath()
    {
        SpecialFolder folder = SpecialFolder.UserProfile;

        string expected = Environment.GetFolderPath(folder);
        string actual = FileSys.UserProfile;

        Assert.AreEqual(expected, actual, $"Expected home path: '{expected}'.");
    }

    /// <summary>
    ///  Assert that the home directory path of the current user exists.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.UserProfile"/>.
    /// </remarks>
    [TestMethod]
    public void UserProfile_Get_HomePathExists()
    {
        string homePath = FileSys.UserProfile;
        bool actual = Directory.Exists(homePath);

        Assert.IsTrue(actual, $"User home path '{homePath}' does not exist.");
    }
#endregion // PropertyTests

#region MethodTests
    /// <summary>
    ///  Assert that an existing input file or directory path returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.Exists(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(HOME_PATH_ALIAS)]
    [DataRow(".")]
    [DataRow(@".\..")]
#if WINDOWS
    [DataRow(@"C:\Users")]
    [DataRow("/Windows/System32/dism.exe")]
    [DataRow(@"C:\Windows/System32\sfc.exe")]
#elif LINUX // LINUX
    [DataRow("/etc")]
    [DataRow(@"\dev\error")]
    [DataRow("/dev/stdout")]
#endif // WINDOWS
    public void Exists_Does_ReturnsTrue(string? path)
    {
        bool actual = FileSys.Exists(path);
        Assert.IsTrue(actual, $"Expected path '{path}' to exist.");
    }

    /// <summary>
    ///  Assert that a nonexistent input file or directory path returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.Exists(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
#if WINDOWS
    [DataRow("/Windows/Desktop")]
    [DataRow(@"C:\Windows\Files\explorer.exe")]
#elif LINUX // LINUX
    [DataRow("/usr/shared")]
    [DataRow(@"\bin\files\run")]
#endif // WINDOWS
    public void Exists_DoesNot_ReturnsFalse(string? path)
    {
        bool actual = FileSys.Exists(path);
        Assert.IsFalse(actual, $"Expected path '{path}' to not exist.");
    }

    /// <summary>
    ///  Assert that a nonexistent input file or directory path returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.Exists(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void Exists_DoesNot_ReturnsFalse()
    {
        string? path = null;
        bool actual = FileSys.Exists(path);

        Assert.IsFalse(actual, "Expected null path to not exist.");
    }

    /// <summary>
    ///  Assert that an existing input file path returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.FileExists(string?)"/>.
    /// </remarks>
    [TestMethod]
#if WINDOWS
    [DataRow("/Windows/explorer.exe")]
    [DataRow(@"C:\Windows/System32\sfc.exe")]
    [DataRow(@"\Windows\System32\dism.exe")]
#elif LINUX // LINUX
    [DataRow("/etc/hosts")]
    [DataRow(@"/bin\sh")]
    [DataRow(@"\dev\stdout")]
#endif // WINDOWS
    public void FileExists_Does_ReturnsTrue(string? path)
    {
        bool actual = FileSys.FileExists(path);
        Assert.IsTrue(actual, $"Expected file path '{path}' to exist.");
    }

    /// <summary>
    ///  Assert that a nonexistent input file path returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.FileExists(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow(HOME_PATH_ALIAS)]
#if WINDOWS
    [DataRow("~/Documents")]
    [DataRow(@"C:\Windows\System32\explorer.exe")]
#elif LINUX // LINUX
    [DataRow("/dev/output")]
    [DataRow(@"\bin\sbin\sh")]
#endif // WINDOWS
    public void FileExists_DoesNot_ReturnsFalse(string? path)
    {
        bool actual = FileSys.FileExists(path);
        Assert.IsFalse(actual, $"Expected file path '{path}' to not exist.");
    }

    /// <summary>
    ///  Assert that a nonexistent input file path returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.FileExists(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void FileExists_DoesNot_ReturnsFalse()
    {
        string? path = null;
        bool actual = FileSys.FileExists(path);

        Assert.IsFalse(actual, "Expected null file path to not exist.");
    }

    /// <summary>
    ///  Assert that an existing input directory path returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.DirectoryExists(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(HOME_PATH_ALIAS)]
    [DataRow("/")]
    [DataRow(".")]
    [DataRow("..")]
    [DataRow(@"..\..")]
    public void DirectoryExists_Does_ReturnsTrue(string? path)
    {
        bool actual = FileSys.DirectoryExists(path);
        Assert.IsTrue(actual, $"Expected directory path '{path}' to exist.");
    }

    /// <summary>
    ///  Assert that a nonexistent input directory path returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.DirectoryExists(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
#if WINDOWS
    [DataRow("~/Uploads")]
    [DataRow(@"\Windows\System32\explorer.exe")]
#elif LINUX // LINUX
    [DataRow("/bin/sh")]
    [DataRow(@"\dev\output")]
#endif // WINDOWS
    public void DirectoryExists_DoesNot_ReturnsFalse(string? path)
    {
        bool actual = FileSys.DirectoryExists(path);
        Assert.IsFalse(actual, $"Expected directory path '{path}' to not exist.");
    }

    /// <summary>
    ///  Assert that a nonexistent input directory path returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="FileSys.DirectoryExists(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void DirectoryExists_DoesNot_ReturnsFalse()
    {
        string? path = null;
        bool actual = FileSys.DirectoryExists(path);

        Assert.IsFalse(actual, "Expected null directory path to not exist.");
    }
#endregion // MethodTests
}
