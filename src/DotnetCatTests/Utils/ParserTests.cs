using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Utils;

namespace DotnetCatTests.Utils;

/// <summary>
///  Unit tests for class <see cref="Parser"/>.
/// </summary>
[TestClass]
public class ParserTests
{
#region MethodTests
    /// <summary>
    ///  Assert that an input command-line argument array containing a
    ///  help flag or flag alias (<c>-?</c>, <c>-h</c>, <c>--help</c>)
    ///  sets the <see cref="CmdLineArgs.Help"/> property to true.
    /// </summary>
    [DataTestMethod]
    [DataRow("-?")]
    [DataRow("-h")]
    [DataRow("--help")]
    [DataRow("-d?", "--listen")]
    [DataRow("-hz", "--debug")]
    [DataRow("-v", "--help")]
    [DataRow("-vp", "22", "-?", "localhost")]
    [DataRow("--exec", "pwsh.exe", "-h", "localhost")]
    [DataRow("--listen", "--help", "localhost")]
    public void Parse_HelpFlag_HelpPropertyTrue(params string[] args)
    {
        Parser parser = new();

        CmdLineArgs cmdArgs = parser.Parse(args);
        bool actual = cmdArgs.Help;

        Assert.IsTrue(actual, "Failed to parse help flag or flag alias");
    }

    /// <summary>
    ///  Assert that an input command-line argument array not containing any
    ///  values sets the <see cref="CmdLineArgs.Help"/> property to true.
    /// </summary>
    [TestMethod]
    public void Parse_NoArguments_HelpPropertyTrue()
    {
        string[] args = [];
        Parser parser = new();

        CmdLineArgs cmdArgs = parser.Parse(args);
        bool actual = cmdArgs.Help;

        Assert.IsTrue(actual, "Help should be true when no arguments are provided");
    }

    /// <summary>
    ///  Assert that an input command-line argument array not containing
    ///  a help flag or flag alias (<c>-?</c>, <c>-h</c>, <c>--help</c>)
    ///  sets the <see cref="CmdLineArgs.Help"/> property to false.
    /// </summary>
    [DataTestMethod]
    [DataRow("-vp", "22", "-e", "pwsh.exe", "192.168.1.100")]
    [DataRow("--verbose", "--send", "~/test.txt", "localhost")]
    [DataRow("--listen", "-vo", "~/recv_data.txt", "-p", "31337")]
    public void Parse_NoHelpFlag_HelpPropertyFalse(params string[] args)
    {
        Parser parser = new();

        CmdLineArgs cmdArgs = parser.Parse(args);
        bool actual = cmdArgs.Help;

        Assert.IsFalse(actual, "Unexpectedly parsed help flag or flag alias");
    }
#endregion // MethodTests
}
