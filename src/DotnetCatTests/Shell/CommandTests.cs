using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Shell;

namespace DotnetCatTests.Shell;

/// <summary>
///  Unit tests for utility class <c>DotnetCat.Shell.Command</c>.
/// </summary>
[TestClass]
public class CommandTests
{
    /// <summary>
    ///  Assert that a valid input environment variable name returns the
    ///  value of the corresponding variable in the local system.
    /// </summary>
    [DataTestMethod]
    [DataRow("PATH")]
    public void GetEnvVariable_ValidEnvVariableName_EqualsExpected(string name)
    {
        string? actualValue = Command.GetEnvVariable(name);
        string? expectedValue = Environment.GetEnvironmentVariable(name);

        Assert.AreEqual(actualValue, expectedValue);
    }

    /// <summary>
    ///  Assert that an invalid input environment variable name returns null.
    /// </summary>
    [DataTestMethod]
    [DataRow("DotnetCatTest_Test")]
    [DataRow("DotnetCatTest_Data")]
    public void GetEnvVariable_InvalidEnvVariableName_ReturnsNull(string name)
    {
        string? varValue = Command.GetEnvVariable(name);
        Assert.IsNull(varValue);
    }

    /// <summary>
    ///  Assert that a null input environment variable name causes an
    ///  <c>ArgumentNullException</c> to be thrown.
    /// </summary>
    [TestMethod]
    public void GetEnvVariable_NullEnvVariableName_ThrowsArgumentNullException()
    {
        string? varName = null;

    #nullable disable
        Func<string> func = () => Command.GetEnvVariable(varName);
    #nullable enable

        Assert.ThrowsException<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that a non-null input shell name returns a
    ///  new <c>ProcessStartInfo</c> object.
    /// </summary>
    [DataTestMethod]
    [DataRow("test")]
    [DataRow("data.exe")]
    public void GetExeStartInfo_NonNullShell_ReturnsNewStartInfo(string shell)
    {
        ProcessStartInfo actual = Command.GetExeStartInfo(shell);
        Assert.IsNotNull(actual);
    }

    /// <summary>
    ///  Assert that a null input shell name causes an
    ///  <c>ArgumentNullException</c> to be thrown.
    /// </summary>
    [TestMethod]
    public void GetExeStartInfo_NullShell_ThrowsArgumentNullException()
    {
        string? shell = null;
        Func<ProcessStartInfo> func = () => Command.GetExeStartInfo(shell);

        Assert.ThrowsException<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that a valid input clear-screen command name returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("clear")]
    [DataRow("cls")]
    [DataRow("Clear-Host")]
    public void IsClearCmd_ValidCommand_ReturnsTrue(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an invalid input clear-screen command name returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("sudo")]
    [DataRow("cat")]
    [DataRow("Get-Location")]
    public void IsClearCmd_InvalidCommand_ReturnsFalse(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsFalse(actual);
    }
}
