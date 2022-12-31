using System.Collections.Generic;
using System.Linq;
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
    ///  Assert that an input command-line argument array containing the
    ///  specified argument flag or flag alias returns the correct index.
    /// </summary>
    [DataTestMethod]
    [DataRow("--send", 's', 0, "-vls", "~/data.txt", "localhost")]
    [DataRow("--send", null, 1, "-vl", "--send", "~/data.txt", "localhost")]
    [DataRow("--output", 'o', 0, "-dlo", "~/data.txt", "localhost")]
    [DataRow("--output", null, 1, "-dl", "--output", "~/data.txt", "localhost")]
    public void IndexOfFlag_MatchingFlag_ReturnsCorrectIndex(string flag,
                                                             char? alias,
                                                             int expected,
                                                             params string[] args) {
        List<string>? argsList = args.ToList();
        int actual = Parser.IndexOfFlag(argsList, flag, alias);

        Assert.AreEqual(actual, expected, $"Expected result index: '{expected}'");
    }

    /// <summary>
    ///  Assert that an input command-line argument array not containing
    ///  the specified argument flag or flag alias returns -1.
    /// </summary>
    [DataTestMethod]
    [DataRow("--send", 's', "-vlo", "~/data.txt", "localhost")]
    [DataRow("--send", null, "-vl", "--output", "~/data.txt", "localhost")]
    [DataRow("--output", 'o', "-vls", "~/data.txt", "localhost")]
    [DataRow("--output", null, "-vl", "--send", "~/data.txt", "localhost")]
    public void IndexOfFlag_NoMatchingFlag_ReturnsNegativeOne(string flag,
                                                              char? alias,
                                                              params string[] args) {
        List<string>? argsList = args.ToList();

        int expected = -1;
        int actual = Parser.IndexOfFlag(argsList, flag, alias);

        Assert.AreEqual(actual, expected, $"Expected result index: '{expected}'");
    }

    /// <summary>
    ///  Assert that an input command-line argument array containing a
    ///  help flag or flag alias (`-?`, `-h`, `--help`) returns true.
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
    public void NeedsHelp_HelpFlag_ReturnsTrue(params string[] args)
    {
        bool actual = Parser.NeedsHelp(args);
        Assert.IsTrue(actual, "Expected a help flag or flag alias");
    }

    /// <summary>
    ///  Assert that an input command-line argument array not containing
    ///  a help flag or flag alias (`-?`, `-h`, `--help`) returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("-vp", "22", "-e", "pwsh.exe")]
    [DataRow("--debug", "--send", "~/data.txt", "localhost")]
    [DataRow("--listen", "-do", "~/recv_data.txt")]
    public void NeedsHelp_NoHelpFlag_ReturnsFalse(params string[] args)
    {
        bool actual = Parser.NeedsHelp(args);
        Assert.IsFalse(actual, "Did not expect a help flag or flag alias");
    }
#endregion // MethodTests
}
