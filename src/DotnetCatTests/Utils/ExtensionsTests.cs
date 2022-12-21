using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Utils;

namespace DotnetCatTests.Utils;

/// <summary>
///  Unit tests for utility class <c>DotnetCat.Utils.Extensions</c>.
/// </summary>
[TestClass]
public class ExtensionsTests
{
    /// <summary>
    ///  Assert that a null input string returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_NullString_ReturnsTrue()
    {
        string? str = null;
        bool actual = str.IsNullOrEmpty();

        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an empty or blank input string returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("")]
    [DataRow("    ")]
    public void IsNullOrEmpty_EmptyOrBlankString_ReturnsTrue(string? str)
    {
        bool actual = str.IsNullOrEmpty();
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that a populated input string returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("testing")]
    [DataRow("  testing")]
    [DataRow("testing  ")]
    public void IsNullOrEmpty_PopulatedString_ReturnsFalse(string? str)
    {
        bool actual = str.IsNullOrEmpty();
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that a null input array returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_NullArray_ReturnsTrue()
    {
        string[]? array = null;
        bool actual = array.IsNullOrEmpty();

        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an empty input array returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_EmptyArray_ReturnsTrue()
    {
        string[] array = Array.Empty<string>();
        bool actual = array.IsNullOrEmpty();

        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that a populated input array returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(1, 2, 3)]
    [DataRow("test", "data")]
    public void IsNullOrEmpty_PopulatedArray_ReturnsFalse(params object[] array)
    {
        bool actual = array.IsNullOrEmpty();
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that a null generic input list returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_NullList_ReturnsTrue()
    {
        List<string>? list = null;
        bool actual = list.IsNullOrEmpty();

        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an empty generic input list returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_EmptyList_ReturnsTrue()
    {
        List<object> list = new();
        bool actual = list.IsNullOrEmpty();

        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that a populated generic input list returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(1, 2, 3)]
    [DataRow("test", "data")]
    public void IsNullOrEmpty_PopulatedList_ReturnsFalse(params object[] array)
    {
        List<object> list = array.ToList();
        bool actual = list.IsNullOrEmpty();

        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that an input string ending with a specific substring returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("test data", " data")]
    [DataRow("test data ", "data ")]
    [DataRow("test data", "test data")]
    public void EndsWithValue_StringDoes_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an input string not ending with a
    ///  specific substring returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(null, "test data")]
    [DataRow("test data", null)]
    [DataRow("test data", " data ")]
    public void EndsWithValue_StringDoesNot_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that an input string starting with a specific substring returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("test data", "test ")]
    [DataRow(" test data ", " test d")]
    [DataRow("test data", "test data")]
    public void StartsWithValue_StringDoes_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an input string not starting with a
    ///  specific substring returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(null, "test data")]
    [DataRow("test data", null)]
    [DataRow("test data", " test ")]
    public void StartsWithValue_StringDoesNot_ReturnsFalse(string? str,
                                                           string? value) {
        bool actual = str.StartsWithValue(value);
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that an input string starting with a specific character returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("test data", 't')]
    [DataRow(" test data ", ' ')]
    public void StartsWithValue_CharDoes_ReturnsTrue(string? str, char value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an input string not starting with a
    ///  specific character returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(null, 't')]
    [DataRow(" test data ", 't')]
    public void StartsWithValue_CharDoesNot_ReturnsFalse(string? str, char value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that an <c>ArgumentNullException</c> is thrown
    ///  when the input array is null.
    /// </summary>
    [DataTestMethod]
    [DataRow("")]
    [DataRow("|")]
    public void Join_NullArray_ThrowsArgumentNullException(string delim)
    {
        string[]? array = null;
        Func<string> func = () => array.Join(delim);

        Assert.ThrowsException<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that the joined output string is equal to the expected result.
    /// </summary>
    [DataTestMethod]
    [DataRow(new object[] { 1, 2, 3 }, "|")]
    [DataRow(new string[] { "test", "data" }, null)]
    public void Join_NonNullArray_EqualsExpected(object[] array, string delim)
    {
        string expected = string.Join(delim, array);
        string actual = array.Join(delim);

        Assert.AreEqual(actual, expected);
    }

    /// <summary>
    ///  Assert that an <c>ArgumentNullException</c> is thrown
    ///  when the input array is null.
    /// </summary>
    [TestMethod]
    public void JoinLines_ArrayIsNull_ThrowsArgumentNullException()
    {
        string[]? array = null;
        Func<string> func = () => array.JoinLines();

        Assert.ThrowsException<ArgumentNullException>(func);
    }
}
