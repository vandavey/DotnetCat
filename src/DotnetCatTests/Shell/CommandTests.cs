using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Shell;
using static DotnetCat.Utils.Constants;

namespace DotnetCatTests.Shell;

/// <summary>
///  Unit tests for utility class <see cref="Command"/>.
/// </summary>
[TestClass]
public class CommandTests
{
#region MethodTests
    /// <summary>
    ///  Assert that an input environment variable name returns the
    ///  value of the corresponding variable in the local system.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Command.EnvVariable(string)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(ENV_VAR_PATH, false)]
    [DataRow("DotnetCatTest_Test", true)]
    [DataRow("DotnetCatTest_Data", true)]
#if WINDOWS
    [DataRow("USERNAME", false)]
    [DataRow("USERPROFILE", false)]
#elif LINUX // LINUX
    [DataRow("HOME", false)]
    [DataRow("USER", false)]
#endif // WINDOWS
    public void EnvVariable_NotNullName_ReturnsExpected(string name, bool expectNull)
    {
        string? expected = expectNull ? null : Environment.GetEnvironmentVariable(name);
        string? actual = Command.EnvVariable(name);

        Assert.AreEqual(expected, actual, $"Unexpected value for '{name}': '{actual}'.");
    }

    /// <summary>
    ///  Assert that a null input environment variable name causes
    ///  an <see cref="ArgumentNullException"/> error to be thrown.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Command.EnvVariable(string)"/>.
    /// </remarks>
    [TestMethod]
    public void EnvVariable_NullName_ThrowsArgumentNullException()
    {
        string? name = null;
        Func<string?> testFunc = () => Command.EnvVariable(name!);

        Assert.Throws<ArgumentNullException>(testFunc);
    }

    /// <summary>
    ///  Assert that a non-null input shell name returns
    ///  a new <see cref="ProcessStartInfo"/> object.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Command.ExeStartInfo(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("test")]
    [DataRow("data.exe")]
    public void ExeStartInfo_NotNullShell_ReturnsNewStartInfo(string? shell)
    {
        ProcessStartInfo? actual = Command.ExeStartInfo(shell);
        Assert.IsNotNull(actual, "Resulting startup information should not be null.");
    }

    /// <summary>
    ///  Assert that a null input shell name causes an
    ///  <see cref="ArgumentNullException"/> error to be thrown.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Command.ExeStartInfo(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void ExeStartInfo_NullShell_ThrowsArgumentNullException()
    {
        string? shell = null;
        Func<ProcessStartInfo> testFunc = () => Command.ExeStartInfo(shell);

        Assert.Throws<ArgumentNullException>(testFunc);
    }

    /// <summary>
    ///  Assert that a valid input clear-screen command name returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Command.IsClearCmd(string)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("clear")]
    [DataRow("cls\n")]
    [DataRow("Clear-Host")]
    public void IsClearCmd_Is_ReturnsTrue(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsTrue(actual, $"'{command}' should be a clear-screen command.");
    }

    /// <summary>
    ///  Assert that an invalid input clear-screen command name returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Command.IsClearCmd(string)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("sudo")]
    [DataRow("cat\n")]
    [DataRow("Get-Location")]
    public void IsClearCmd_IsNot_ReturnsFalse(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsFalse(actual, $"'{command}' should not be a clear-screen command.");
    }
#endregion // MethodTests
}
