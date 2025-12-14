using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Utils;

namespace DotnetCatTests.Utils;

/// <summary>
///  Unit tests for utility class <see cref="Extensions"/>.
/// </summary>
[TestClass]
public class ExtensionsTests
{
#region MethodTests
    /// <summary>
    ///  Assert that an input string ending with a specific character returns true.
    /// </summary>
    [TestMethod]
    [DataRow("test data", 'a')]
    [DataRow(" test data ", ' ')]
    [DataRow("test data 1", '1')]
    [DataRow("test data\0", '\0')]
    public void EndsWithValue_CharDoes_ReturnsTrue(string? str, char value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to end with '{value}'");
    }

    /// <summary>
    ///  Assert that an input string not ending with a specific character returns false.
    /// </summary>
    [TestMethod]
    [DataRow(null, 't')]
    [DataRow(" test data", ' ')]
    [DataRow("test data", 't')]
    [DataRow("\0test data", '\0')]
    public void EndsWithValue_CharDoesNot_ReturnsFalse(string? str, char value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not end with '{value}'");
    }

    /// <summary>
    ///  Assert that an input string ending with a specific substring returns true.
    /// </summary>
    [TestMethod]
    [DataRow("test data", " data")]
    [DataRow("test data ", "data ")]
    [DataRow("test data", "test data")]
    public void EndsWithValue_StringDoes_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to end with '{value}'");
    }

    /// <summary>
    ///  Assert that an input string not ending with a specific substring returns false.
    /// </summary>
    [TestMethod]
    [DataRow(null, "test data")]
    [DataRow("test data", null)]
    [DataRow("test data", " data ")]
    public void EndsWithValue_StringDoesNot_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not end with '{value}'");
    }

    /// <summary>
    ///  Assert that a null input string returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_NullString_ReturnsTrue()
    {
        string? str = null;
        bool actual = str.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected null string to be null or empty");
    }

    /// <summary>
    ///  Assert that an empty or blank input string returns true.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("    ")]
    public void IsNullOrEmpty_EmptyOrBlankString_ReturnsTrue(string? str)
    {
        bool actual = str.IsNullOrEmpty();
        Assert.IsTrue(actual, "Expected empty/blank string to be null or empty");
    }

    /// <summary>
    ///  Assert that a populated input string returns false.
    /// </summary>
    [TestMethod]
    [DataRow("testing")]
    [DataRow("  testing")]
    [DataRow("testing  ")]
    public void IsNullOrEmpty_PopulatedString_ReturnsFalse(string? str)
    {
        bool actual = str.IsNullOrEmpty();
        Assert.IsFalse(actual, "Expected populated string to not be null or empty");
    }

    /// <summary>
    ///  Assert that a null input array returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_NullArray_ReturnsTrue()
    {
        string[]? array = null;
        bool actual = array.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected null array to be null or empty");
    }

    /// <summary>
    ///  Assert that an empty input array returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_EmptyArray_ReturnsTrue()
    {
        string[] array = [];
        bool actual = array.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected empty array to be null or empty");
    }

    /// <summary>
    ///  Assert that a populated input array returns false.
    /// </summary>
    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow("test", "data")]
    public void IsNullOrEmpty_PopulatedArray_ReturnsFalse(params object[] array)
    {
        bool actual = array.IsNullOrEmpty();
        Assert.IsFalse(actual, "Expected populated array to not be null or empty");
    }

    /// <summary>
    ///  Assert that a null generic input list returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_NullList_ReturnsTrue()
    {
        List<string>? list = null;
        bool actual = list.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected null list to be null or empty");
    }

    /// <summary>
    ///  Assert that an empty generic input list returns true.
    /// </summary>
    [TestMethod]
    public void IsNullOrEmpty_EmptyList_ReturnsTrue()
    {
        List<object> list = [];
        bool actual = list.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected empty list to be null or empty");
    }

    /// <summary>
    ///  Assert that a populated generic input list returns false.
    /// </summary>
    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow("test", "data")]
    public void IsNullOrEmpty_PopulatedList_ReturnsFalse(params object[] array)
    {
        List<object> list = [.. array];
        bool actual = list.IsNullOrEmpty();

        Assert.IsFalse(actual, "Expected populated list to not be null or empty");
    }

    /// <summary>
    ///  Assert that an input string whose value is equal to
    ///  another string when casing is ignored returns true.
    /// </summary>
    [TestMethod]
    [DataRow("", "")]
    [DataRow("  ", "  ")]
    [DataRow("test", "TEST")]
    [DataRow("TEST", "test")]
    [DataRow("tEsT", "TeSt")]
    public void NoCaseEquals_EqualStrings_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.NoCaseEquals(value);
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an input string whose value is not equal to
    ///  another string when casing is ignored returns false.
    /// </summary>
    [TestMethod]
    [DataRow(null, "test")]
    [DataRow("test", null)]
    [DataRow("tEsT", "DaTa")]
    public void NoCaseEquals_NotEqualStrings_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.NoCaseEquals(value);
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that a null input string compared to another null string returns true.
    /// </summary>
    [TestMethod]
    public void NoCaseEquals_NullStrings_ReturnsTrue()
    {
        string? str = null;
        bool actual = str.NoCaseEquals(null);

        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an input string starting with a specific character returns true.
    /// </summary>
    [TestMethod]
    [DataRow("test data", 't')]
    [DataRow(" test data ", ' ')]
    [DataRow("1 test data", '1')]
    [DataRow("\0test data", '\0')]
    public void StartsWithValue_CharDoes_ReturnsTrue(string? str, char value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to start with '{value}'");
    }

    /// <summary>
    ///  Assert that an input string not starting with a specific character returns false.
    /// </summary>
    [TestMethod]
    [DataRow(null, 't')]
    [DataRow(" test data ", 't')]
    [DataRow("test data ", ' ')]
    [DataRow("test data\0", '\0')]
    public void StartsWithValue_CharDoesNot_ReturnsFalse(string? str, char value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not start with '{value}'");
    }

    /// <summary>
    ///  Assert that an input string starting with a specific substring returns true.
    /// </summary>
    [TestMethod]
    [DataRow("test data", "test ")]
    [DataRow(" test data ", " test d")]
    [DataRow("test data", "test data")]
    public void StartsWithValue_StringDoes_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to start with '{value}'");
    }

    /// <summary>
    ///  Assert that an input string not starting with a specific substring returns false.
    /// </summary>
    [TestMethod]
    [DataRow(null, "test data")]
    [DataRow("test data", null)]
    [DataRow("test data", " test ")]
    public void StartsWithValue_StringDoesNot_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not start with '{value}'");
    }

    /// <summary>
    ///  Assert that an <see cref="ArgumentNullException"/>
    ///  is thrown when the input array is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("|")]
    public void Join_NullArray_ThrowsArgumentNullException(string delim)
    {
        string[]? array = null;
        Func<string> func = () => array.Join(delim);

        Assert.Throws<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that the joined output string is equal to the expected result.
    /// </summary>
    [TestMethod]
    [DataRow(new object[] { 1, 2, 3 }, "|")]
    [DataRow(new string[] { "test", "data" }, null)]
    public void Join_NonNullArray_EqualsExpected(object[] array, string? delim)
    {
        string expected = string.Join(delim, array);
        string actual = array.Join(delim);

        Assert.AreEqual(expected, actual, $"Expected result string: '{expected}'");
    }

    /// <summary>
    ///  Assert that an <see cref="ArgumentNullException"/>
    ///  is thrown when the input array is null.
    /// </summary>
    [TestMethod]
    public void JoinLines_NullArray_ThrowsArgumentNullException()
    {
        string[]? array = null;
        Assert.Throws<ArgumentNullException>(array.JoinLines);
    }

    /// <summary>
    ///  Assert that a populated input array returns the expected tuple enumerable.
    /// </summary>
    [TestMethod]
    [DataRow(0, 1, 2, 3)]
    [DataRow("test", "data")]
    [DataRow('t', 'e', 's', 't')]
    public void Enumerate_PopulatedArray_ReturnsExpected(params object[] array)
    {
        IEnumerable<object>? values = array;

        (int, object)[] expected = [.. values.Select((v, i) => (i, v))];
        (int, object)[] actual = [.. values.Enumerate()];

        CollectionAssert.AreEquivalent(actual, expected, "Unexpected results");
    }

    /// <summary>
    ///  Assert that an empty input array returns an empty tuple enumerable.
    /// </summary>
    /// <remarks>
    ///  Array of type <see cref="ValueTuple{T1, T2}"/> is used in place of
    ///  <see cref="IEnumerable{T}"/> for compatibility with
    ///  <see cref="CollectionAssert.AreEquivalent"/> assertions.
    /// </remarks>
    [TestMethod]
    public void Enumerate_EmptyArray_ReturnsEmpty()
    {
        IEnumerable<string>? values = [];
        (int, string)[] expected = [];

        IEnumerable<(int, string)> actualEnumerable = values.Enumerate();
        (int, string)[] actual = [.. actualEnumerable];

        CollectionAssert.AreEquivalent(actual, expected, "Unexpected results");
    }

    /// <summary>
    ///  Assert that a null input enumerable returns an empty tuple enumerable.
    /// </summary>
    /// <remarks>
    ///  Array of type <see cref="ValueTuple{T1, T2}"/> is used in place of
    ///  <see cref="IEnumerable{T}"/> for compatibility with
    ///  <see cref="CollectionAssert.AreEquivalent"/> assertions.
    /// </remarks>
    [TestMethod]
    public void Enumerate_NullEnumerable_ReturnsEmpty()
    {
        IEnumerable<string>? values = null;

        (int, string)[] expected = [];
        (int, string)[] actual = [.. values.Enumerate()];

        CollectionAssert.AreEquivalent(actual, expected, "Unexpected results");
    }
#endregion // MethodTests
}
