using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Shell;

namespace DotnetCatTests.Shell;

/// <summary>
///  Unit tests for utility class <see cref="Command"/>.
/// </summary>
[TestClass]
public class CommandTests
{
#region MethodTests
    /// <summary>
    ///  Assert that a valid input environment variable name returns the
    ///  value of the corresponding variable in the local system.
    /// </summary>
    [TestMethod]
    [DataRow("PATH")]
#if WINDOWS
    [DataRow("USERNAME")]
    [DataRow("USERPROFILE")]
#elif LINUX
    [DataRow("HOME")]
    [DataRow("USER")]
#endif // WINDOWS
    public void EnvVariable_ValidEnvVariableName_ReturnsExpected(string name)
    {
        string? expected = Environment.GetEnvironmentVariable(name);
        string? actual = Command.EnvVariable(name);

        Assert.AreEqual(expected, actual, $"Incorrect value for variable '{name}'");
    }

    /// <summary>
    ///  Assert that an invalid input environment variable name returns null.
    /// </summary>
    [TestMethod]
    [DataRow("DotnetCatTest_Test")]
    [DataRow("DotnetCatTest_Data")]
    public void EnvVariable_InvalidEnvVariableName_ReturnsNull(string name)
    {
        string? actual = Command.EnvVariable(name);
        Assert.IsNull(actual, $"Value for variable '{name}' should be null");
    }

    /// <summary>
    ///  Assert that a null input environment variable name causes
    ///  an <see cref="ArgumentNullException"/> to be thrown.
    /// </summary>
    [TestMethod]
    public void EnvVariable_NullEnvVariableName_ThrowsArgumentNullException()
    {
        string? name = null;

    #nullable disable
        Func<string> func = () => Command.EnvVariable(name);
    #nullable enable

        Assert.Throws<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that a non-null input shell name returns
    ///  a new <see cref="ProcessStartInfo"/> object.
    /// </summary>
    [TestMethod]
    [DataRow("test")]
    [DataRow("data.exe")]
    public void ExeStartInfo_NonNullShell_ReturnsNewStartInfo(string shell)
    {
        ProcessStartInfo? actual = Command.ExeStartInfo(shell);
        Assert.IsNotNull(actual, "Resulting startup information should not be null");
    }

    /// <summary>
    ///  Assert that a null input shell name causes an
    ///  <see cref="ArgumentNullException"/> to be thrown.
    /// </summary>
    [TestMethod]
    public void ExeStartInfo_NullShell_ThrowsArgumentNullException()
    {
        string? shell = null;
        Func<ProcessStartInfo> func = () => Command.ExeStartInfo(shell);

        Assert.Throws<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that a valid input clear-screen command name returns true.
    /// </summary>
    [TestMethod]
    [DataRow("clear")]
    [DataRow("cls\n")]
    [DataRow("Clear-Host")]
    public void IsClearCmd_ValidCommand_ReturnsTrue(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsTrue(actual, $"'{command}' should be a clear-screen command");
    }

    /// <summary>
    ///  Assert that an invalid input clear-screen command name returns false.
    /// </summary>
    [TestMethod]
    [DataRow("sudo")]
    [DataRow("cat\n")]
    [DataRow("Get-Location")]
    public void IsClearCmd_InvalidCommand_ReturnsFalse(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsFalse(actual, $"'{command}' should not be a clear-screen command");
    }
#endregion // MethodTests
}
