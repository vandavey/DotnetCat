using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Shell;
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
    ///  Assert that an action is correctly executed
    ///  against values in a non-null input collection.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.ForEach{T}(IEnumerable{T}?, Action{T})"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(0)]
    [DataRow([])]
    [DataRow(2U, 4U)]
    [DataRow("this", "is", "test", "data")]
    [DataRow(typeof(int), typeof(string), typeof(double))]
    public void ForEach_NotNullCollection_CorrectAction(params object[] values)
    {
        string[] expected = [.. values.Select(v => $"Test value: {v}")];
        List<string> actual = [];

        Action<object> action = v => actual.Add($"Test value: {v}");
        values.ForEach(action);

        CollectionAssert.AreEquivalent(expected, actual, "Unexpected results.");
    }

    /// <summary>
    ///  Assert that no action is executed against values in a null input collection.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.ForEach{T}(IEnumerable{T}?, Action{T})"/>.
    /// </remarks>
    [TestMethod]
    public void ForEach_NullCollection_NoAction()
    {
        object[]? values = null;
        List<string> actual = [];

        Action<object> action = v => actual.Add($"Test value: {v}");
        values.ForEach(action);

        Assert.IsEmpty(actual, "No actions should be executed.");
    }

    /// <summary>
    ///  Assert that a line is written when an input condition is true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.WriteLineIf(TextWriter, bool)"/>.
    /// </remarks>
    [TestMethod]
    public void WriteLineIf_ConditionTrue_DoesWrite()
    {
        bool condition = true;
        StringBuilder data = new();

        using TextWriter writer = new StringWriter(data);
        writer.WriteLineIf(condition);

        string expected = SysInfo.Eol;
        string actual = data.ToString();

        Assert.AreEqual(expected, actual, $"Expected result: '{expected}'.");
    }

    /// <summary>
    ///  Assert that a line is not written when an input condition is false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.WriteLineIf(TextWriter, bool)"/>.
    /// </remarks>
    [TestMethod]
    public void WriteLineIf_ConditionFalse_DoesNotWrite()
    {
        bool condition = false;
        StringBuilder data = new();

        using TextWriter writer = new StringWriter(data);
        writer.WriteLineIf(condition);

        string expected = string.Empty;
        string actual = data.ToString();

        Assert.AreEqual(expected, actual, "Expected empty string result.");
    }

    /// <summary>
    ///  Assert that a line is written when an input condition is true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.WriteLineIf{T}(TextWriter, bool, T?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(0)]
    [DataRow(null)]
    [DataRow(16.0)]
    [DataRow(0x0010U)]
    [DataRow("testing")]
    [DataRow(["test", "data"])]
    public void WriteLineIf_ConditionTrue_DoesWrite(object? obj)
    {
        bool condition = true;
        StringBuilder data = new();

        using TextWriter writer = new StringWriter(data);
        writer.WriteLineIf(condition, obj);

        string expected = $"{obj}{SysInfo.Eol}";
        string actual = data.ToString();

        Assert.AreEqual(expected, actual, $"Expected result '{expected}'.");
    }

    /// <summary>
    ///  Assert that a line is not written when an input condition is false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.WriteLineIf{T}(TextWriter, bool, T?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(0)]
    [DataRow(null)]
    [DataRow(16.0)]
    [DataRow(0x0010U)]
    [DataRow("testing")]
    [DataRow(["test", "data"])]
    public void WriteLineIf_ConditionFalse_DoesNotWrite(object? obj)
    {
        bool condition = false;
        StringBuilder data = new();

        using TextWriter writer = new StringWriter(data);
        writer.WriteLineIf(condition, obj);

        string expected = string.Empty;
        string actual = data.ToString();

        Assert.AreEqual(expected, actual, "Expected empty string result.");
    }

    /// <summary>
    ///  Assert that an input collection containing at least
    ///  the same values as another collection returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.ContainsAll{T}(ICollection{T}?, IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(new object[] { 0 }, new object[] { 0 })]
    [DataRow(new object[] { false, true }, new object[] { false })]
    [DataRow(new object[] { 1L, 2L, 3L }, new object[] { 1L, 3L })]
    [DataRow(new object[] { "test", "data" }, new object[] { "test" })]
    [DataRow(new object[] { "test", "data" }, new object[] { "test", "data" })]
    public void ContainsAll_Does_ReturnsTrue(ICollection<object>? collection,
                                             IEnumerable<object>? values)
    {
        bool actual = collection.ContainsAll(values);
        Assert.IsTrue(actual, "All values were not found in collection.");
    }

    /// <summary>
    ///  Assert that an input collection not containing at least
    ///  the same values as another collection returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.ContainsAll{T}(ICollection{T}?, IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(null, new object[] { })]
    [DataRow(new object[] { }, null)]
    [DataRow(null, new object[] { 2U })]
    [DataRow(new object[] { 64, 128 }, null)]
    [DataRow(new object[] { }, new object[] { })]
    [DataRow(new object[] { }, new object[] { 1L, 2L, 3L })]
    [DataRow(new object[] { false, true }, new object[] { })]
    [DataRow(new object[] { "test" }, new object[] { "test", "data" })]
    public void ContainsAll_DoesNot_ReturnsFalse(ICollection<object>? collection,
                                                 IEnumerable<object>? values)
    {
        bool actual = collection.ContainsAll(values);
        Assert.IsFalse(actual, "All values were found in collection.");
    }

    /// <summary>
    ///  Assert that an input string ending with a single
    ///  or double quotation mark character returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithQuote(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("'")]
    [DataRow(" '")]
    [DataRow(" \"")]
    [DataRow("test data'")]
    [DataRow("test data\"")]
    public void EndsWithQuote_Does_ReturnsTrue(string? str)
    {
        bool actual = str.EndsWithQuote();
        Assert.IsTrue(actual, $"Expected '{str}' to end with quote.");
    }

    /// <summary>
    ///  Assert that an input string not ending with a single
    ///  or double quotation mark character returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithQuote(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow("' ")]
    [DataRow("\" ")]
    [DataRow("test data")]
    [DataRow("\"test data")]
    public void EndsWithQuote_DoesNot_ReturnsFalse(string? str)
    {
        bool actual = str.EndsWithQuote();
        Assert.IsFalse(actual, $"Expected '{str}' to not end with quote.");
    }

    /// <summary>
    ///  Assert that an input string not ending with a single
    ///  or double quotation mark character returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithQuote(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void EndsWithQuote_DoesNot_ReturnsFalse()
    {
        string? str = null;
        bool actual = str.EndsWithQuote();

        Assert.IsFalse(actual, "Null string should not end with quote.");
    }

    /// <summary>
    ///  Assert that an input string ending with a specific character returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithValue(string?, char)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("test data", 'a')]
    [DataRow(" test data ", ' ')]
    [DataRow("test data 1", '1')]
    [DataRow("test data\0", '\0')]
    public void EndsWithValue_Does_ReturnsTrue(string? str, char value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to end with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string not ending with a specific character returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithValue(string?, char)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(null, 't')]
    [DataRow(" test data", ' ')]
    [DataRow("test data", 't')]
    [DataRow("\0test data", '\0')]
    public void EndsWithValue_DoesNot_ReturnsFalse(string? str, char value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not end with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string ending with a specific substring returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithValue(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("test data", " data")]
    [DataRow("test data ", "data ")]
    [DataRow("test data", "test data")]
    public void EndsWithValue_Does_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to end with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string not ending with a specific substring returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EndsWithValue(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(null, "test data")]
    [DataRow("test data", null)]
    [DataRow("test data", " data ")]
    public void EndsWithValue_DoesNot_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.EndsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not end with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input argument whose value is
    ///  equal to any value in a collection returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EqualsAny{T}(T?, IEnumerable{T})"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(0, new object[] { 0, 1 })]
    [DataRow(32U, new object[] { 16U, 32U, 64U })]
    [DataRow("", new object[] { "some", "", "data" })]
    [DataRow(null, new object?[] { "some", null, "data" })]
    [DataRow("data", new object?[] { null, "test", "data" })]
    [DataRow(ArgType.Exec, new object[] { ArgType.Port, ArgType.Exec })]
    public void EqualsAny_Does_ReturnsTrue(object? obj, IEnumerable<object?> values)
    {
        bool actual = obj.EqualsAny(values);
        Assert.IsTrue(actual, $"No equal value found: '{obj}'.");
    }

    /// <summary>
    ///  Assert that an input argument whose value is not
    ///  equal to any value in a collection returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.EqualsAny{T}(T?, IEnumerable{T})"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(5, new object[] { 0, 1 })]
    [DataRow(4U, new object[] { 16U, 32U, 64U })]
    [DataRow("", new object[] { "some", "test", "data" })]
    [DataRow(null, new object?[] { "more", "test", "data" })]
    [DataRow("test", new object?[] { null, "more", "data" })]
    [DataRow(ArgType.Help, new object[] { ArgType.Port, ArgType.Listen })]
    public void EqualsAny_DoesNot_ReturnsFalse(object? obj, IEnumerable<object?> values)
    {
        bool actual = obj.EqualsAny(values);
        Assert.IsFalse(actual, $"Equal value found: '{obj}'.");
    }

    /// <summary>
    ///  Assert that an input string whose value is equal to
    ///  another string when casing is ignored returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IgnCaseEquals(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("", "")]
    [DataRow("  ", "  ")]
    [DataRow("test", "TEST")]
    [DataRow("TEST", "test")]
    [DataRow("tEsT", "TeSt")]
    public void IgnCaseEquals_Does_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.IgnCaseEquals(value);
        Assert.IsTrue(actual, $"Expected '{str}' to equal '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string whose value is equal to
    ///  another string when casing is ignored returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IgnCaseEquals(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    public void IgnCaseEquals_Does_ReturnsTrue()
    {
        string? str = null;
        bool actual = str.IgnCaseEquals(null);

        Assert.IsTrue(actual, "Expected null to equal null.");
    }

    /// <summary>
    ///  Assert that an input string whose value is not equal to
    ///  another string when casing is ignored returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IgnCaseEquals(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(null, "test")]
    [DataRow("test", null)]
    [DataRow("tEsT", "DaTa")]
    public void IgnCaseEquals_DoesNot_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.IgnCaseEquals(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not equal '{value}'.");
    }

    /// <summary>
    ///  Assert that an input value type whose value is default returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsDefault{T}(T?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(bool))]
    [DataRow(typeof(byte))]
    [DataRow(typeof(int))]
    [DataRow(typeof(uint))]
    [DataRow(typeof(long))]
    [DataRow(typeof(double))]
    [DataRow(typeof(ArgType))]
    public void IsDefault_ValueTypeIs_ReturnsTrue(Type type)
    {
        Type classType = typeof(Extensions);
        MethodInfo? methodInfo = classType.GetMethod(nameof(Extensions.IsDefault));
        MethodInfo isDefault = methodInfo!.MakeGenericMethod(type);

        object? obj = Activator.CreateInstance(type);
        bool actual = (bool)isDefault.Invoke(null, [obj])!;

        Assert.IsTrue(actual, "Default value should return true.");
    }

    /// <summary>
    ///  Assert that an input value type whose value is not default returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsDefault{T}(T?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(bool), true)]
    [DataRow(typeof(byte), (byte)1)]
    [DataRow(typeof(int), 2)]
    [DataRow(typeof(uint), 4U)]
    [DataRow(typeof(long), 8L)]
    [DataRow(typeof(double), 16.0)]
    [DataRow(typeof(ArgType), ArgType.Exec)]
    public void IsDefault_ValueTypeIsNot_ReturnsFalse(Type type, object? obj)
    {
        Type classType = typeof(Extensions);
        MethodInfo? methodInfo = classType.GetMethod(nameof(Extensions.IsDefault));
        MethodInfo isDefault = methodInfo!.MakeGenericMethod(type);

        bool actual = (bool)isDefault.Invoke(null, [obj])!;

        Assert.IsFalse(actual, "Non-default value should return false.");
    }

    /// <summary>
    ///  Assert that an input reference type whose value is default returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsDefault{T}(T?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(object))]
    [DataRow(typeof(Exception))]
    [DataRow(typeof(Parser))]
    [DataRow(typeof(CmdLineArgs))]
    public void IsDefault_RefTypeIs_ReturnsTrue(Type type)
    {
        Type classType = typeof(Extensions);
        MethodInfo? methodInfo = classType.GetMethod(nameof(Extensions.IsDefault));
        MethodInfo isDefault = methodInfo!.MakeGenericMethod(type);

        object? value = null;
        bool actual = (bool)isDefault.Invoke(null, [value])!;

        Assert.IsTrue(actual, "Default value should return true.");
    }

    /// <summary>
    ///  Assert that an input reference type whose value is not default returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsDefault{T}(T?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(object))]
    [DataRow(typeof(Exception))]
    [DataRow(typeof(Parser))]
    [DataRow(typeof(CmdLineArgs))]
    public void IsDefault_RefTypeIsNot_ReturnsFalse(Type type)
    {
        Type classType = typeof(Extensions);
        MethodInfo? methodInfo = classType.GetMethod(nameof(Extensions.IsDefault));
        MethodInfo isDefault = methodInfo!.MakeGenericMethod(type);

        object? value = Activator.CreateInstance(type);
        bool actual = (bool)isDefault.Invoke(null, [value])!;

        Assert.IsFalse(actual, "Non-default value should return false.");
    }

    /// <summary>
    ///  Assert that a null input string returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsNullOrEmpty(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void IsNullOrEmpty_StringIs_ReturnsTrue()
    {
        string? str = null;
        bool actual = str.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected null string to be null or empty.");
    }

    /// <summary>
    ///  Assert that an empty or blank input string returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsNullOrEmpty(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow("    ")]
    public void IsNullOrEmpty_StringIs_ReturnsTrue(string? str)
    {
        bool actual = str.IsNullOrEmpty();
        Assert.IsTrue(actual, "Expected empty/blank string to be null or empty.");
    }

    /// <summary>
    ///  Assert that a populated input string returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsNullOrEmpty(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("testing")]
    [DataRow("  testing")]
    [DataRow("testing  ")]
    public void IsNullOrEmpty_StringIsNot_ReturnsFalse(string? str)
    {
        bool actual = str.IsNullOrEmpty();
        Assert.IsFalse(actual, "Expected populated string to not be null or empty.");
    }

    /// <summary>
    ///  Assert that a null input collection returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsNullOrEmpty{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    public void IsNullOrEmpty_CollectionIs_ReturnsTrue()
    {
        List<string>? values = null;
        bool actual = values.IsNullOrEmpty();

        Assert.IsTrue(actual, "Expected null collection to be null or empty.");
    }

    /// <summary>
    ///  Assert that an empty input collection returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsNullOrEmpty{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(new object[0])]
    public void IsNullOrEmpty_CollectionIs_ReturnsTrue(object[]? values)
    {
        bool actual = values.IsNullOrEmpty();
        Assert.IsTrue(actual, "Expected empty collection to be null or empty.");
    }

    /// <summary>
    ///  Assert that a populated input collection returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsNullOrEmpty{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow("test", "data")]
    [DataRow(1U, 2U, 4U, 16U, 32U, 64U)]
    public void IsNullOrEmpty_CollectionIsNot_ReturnsFalse(params object[] array)
    {
        List<object>? values = [.. array];
        bool actual = values.IsNullOrEmpty();

        Assert.IsFalse(actual, "Expected populated collection to not be null or empty.");
    }

    /// <summary>
    ///  Assert that an input type that is a value tuple returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsValueTuple(Type)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof((byte, uint)))]
    [DataRow(typeof((long, double, string)))]
    [DataRow(typeof((object, decimal, Exception, ArgType)))]
    public void IsValueTuple_Is_ReturnsTrue(Type type)
    {
        bool actual = type.IsValueTuple();
        Assert.IsTrue(actual, $"Expected '{type}' to be a value tuple.");
    }

    /// <summary>
    ///  Assert that an input type that is not a value tuple returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.IsValueTuple(Type)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(int))]
    [DataRow(typeof(string[]))]
    [DataRow(typeof(List<double>))]
    [DataRow(typeof(IEnumerable<object>))]
    public void IsValueTuple_IsNot_ReturnsFalse(Type type)
    {
        bool actual = type.IsValueTuple();
        Assert.IsFalse(actual, $"Expected '{type}' to not be a value tuple.");
    }

    /// <summary>
    ///  Assert that an input string starting with a single
    ///  or double quotation mark character returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithQuote(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("'")]
    [DataRow("' ")]
    [DataRow("\" ")]
    [DataRow("'test data")]
    [DataRow("\"test data")]
    public void StartsWithQuote_Does_ReturnsTrue(string? str)
    {
        bool actual = str.StartsWithQuote();
        Assert.IsTrue(actual, $"Expected '{str}' to start with quote.");
    }

    /// <summary>
    ///  Assert that an input string not starting with a single
    ///  or double quotation mark character returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithQuote(string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow(" '")]
    [DataRow(" \"")]
    [DataRow("test data")]
    [DataRow("test data\"")]
    public void StartsWithQuote_DoesNot_ReturnsFalse(string? str)
    {
        bool actual = str.StartsWithQuote();
        Assert.IsFalse(actual, $"Expected '{str}' to not start with quote.");
    }

    /// <summary>
    ///  Assert that an input string not starting with a single
    ///  or double quotation mark character returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithQuote(string?)"/>.
    /// </remarks>
    [TestMethod]
    public void StartsWithQuote_DoesNot_ReturnsFalse()
    {
        string? str = null;
        bool actual = str.StartsWithQuote();

        Assert.IsFalse(actual, "Null string should not start with quote.");
    }

    /// <summary>
    ///  Assert that an input string starting with a specific character returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithValue(string?, char)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("test data", 't')]
    [DataRow(" test data ", ' ')]
    [DataRow("1 test data", '1')]
    [DataRow("\0test data", '\0')]
    public void StartsWithValue_Does_ReturnsTrue(string? str, char value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to start with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string not starting with a specific character returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithValue(string?, char)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(null, 't')]
    [DataRow(" test data ", 't')]
    [DataRow("test data ", ' ')]
    [DataRow("test data\0", '\0')]
    public void StartsWithValue_DoesNot_ReturnsFalse(string? str, char value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not start with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string starting with a specific substring returns true.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithValue(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("test data", "test ")]
    [DataRow(" test data ", " test d")]
    [DataRow("test data", "test data")]
    public void StartsWithValue_Does_ReturnsTrue(string? str, string? value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsTrue(actual, $"Expected '{str}' to start with '{value}'.");
    }

    /// <summary>
    ///  Assert that an input string not starting with a specific substring returns false.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.StartsWithValue(string?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(null, "test data")]
    [DataRow("test data", null)]
    [DataRow("test data", " test ")]
    public void StartsWithValue_DoesNot_ReturnsFalse(string? str, string? value)
    {
        bool actual = str.StartsWithValue(value);
        Assert.IsFalse(actual, $"Expected '{str}' to not start with '{value}'.");
    }

    /// <summary>
    ///  Assert that a joined input collection returns the expected result string.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.Join{T}(IEnumerable{T}?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(new object[0], "|")]
    [DataRow(new object[] { 1, 2, 3 }, "|")]
    [DataRow(new string[] { "test", "data" }, null)]
    public void Join_NotNullCollection_ReturnsExpected(object[]? values, string? delim)
    {
        string expected = string.Join(delim, values!);
        string actual = values.Join(delim);

        Assert.AreEqual(expected, actual, $"Expected result string: '{expected}'.");
    }

    /// <summary>
    ///  Assert that a null input collection causes an
    ///  <see cref="ArgumentNullException"/> error to be thrown.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.Join{T}(IEnumerable{T}?, string?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("")]
    [DataRow("|")]
    public void Join_NullCollection_ThrowsArgumentNullException(string? delim)
    {
        string[]? values = null;
        Func<string> testFunc = () => values.Join(delim);

        Assert.Throws<ArgumentNullException>(testFunc);
    }

    /// <summary>
    ///  Assert that a joined input collection returns the expected result string.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.JoinLines{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(new object[0])]
    [DataRow([1, 2, 3])]
    [DataRow(["test", "data"])]
    public void JoinLines_NotNullCollection_ReturnsExpected(object[]? values)
    {
        string expected = string.Join(SysInfo.Eol, values!);
        string actual = values.JoinLines();

        Assert.AreEqual(expected, actual, $"Expected result string: '{expected}'.");
    }

    /// <summary>
    ///  Assert that a null input collection causes an
    ///  <see cref="ArgumentNullException"/> error to be thrown.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.JoinLines{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    public void JoinLines_NullCollection_ThrowsArgumentNullException()
    {
        string[]? values = null;
        Assert.Throws<ArgumentNullException>(values.JoinLines);
    }

    /// <summary>
    ///  Assert that a non-null input collection returns the expected tuple enumerable.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.Enumerate{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    [DataRow(new object[0])]
    [DataRow([0, 1, 2, 3])]
    [DataRow(["test", "data"])]
    [DataRow(['t', 'e', 's', 't'])]
    public void Enumerate_NotNullCollection_ReturnsExpected(object[]? values)
    {
        (int, object)[] expected = [.. values!.Select((v, i) => (i, v))];
        (int, object)[] actual = [.. values.Enumerate()];

        CollectionAssert.AreEquivalent(expected, actual, "Unexpected results.");
    }

    /// <summary>
    ///  Assert that a null input collection returns an empty tuple enumerable.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="Extensions.Enumerate{T}(IEnumerable{T}?)"/>.
    /// </remarks>
    [TestMethod]
    public void Enumerate_NullCollection_ReturnsEmpty()
    {
        IEnumerable<string>? values = null;

        (int, string)[] expected = [];
        (int, string)[] actual = [.. values.Enumerate()];

        CollectionAssert.AreEquivalent(expected, actual, "Unexpected results.");
    }

    /// <summary>
    ///  Assert that a non-null input collection returns
    ///  the expected tuple enumerable when filtered.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="
    ///      Extensions.Enumerate{T}(IEnumerable{T}?, Func{ValueTuple{int, T}, bool})
    ///  "/>.
    /// </remarks>
    [TestMethod]
    [DataRow(new double[0])]
    [DataRow(new double[] { 0, 1, 2, 3, 4, 5, 6 })]
    [DataRow(new double[] { 1, 2, 4, 8, 16, 32, 64 })]
    [DataRow(new double[] { 1, 3, 9, 27, 81, 243, 729 })]
    [DataRow(new double[] { 0.0, 0.5, 1, 1.5, 2, 2.5, 3 })]
    public void Enumerate_FilteredNotNullCollection_ReturnsExpected(double[]? values)
    {
        Func<(int, double Value), bool> filter = t => t.Value % 2.0 == 0.0;
        IEnumerable<(int, double)> enumeratedValues = values!.Select((v, i) => (i, v));

        (int, double)[] expected = [.. enumeratedValues.Where(filter)];
        (int, double)[] actual = [.. values.Enumerate(filter)];

        CollectionAssert.AreEquivalent(expected, actual, "Unexpected results.");
    }

    /// <summary>
    ///  Assert that a null input collection returns
    ///  an empty tuple enumerable when filtered.
    /// </summary>
    /// <remarks>
    ///  Tests <see cref="
    ///      Extensions.Enumerate{T}(IEnumerable{T}?, Func{ValueTuple{int, T}, bool})
    ///  "/>.
    /// </remarks>
    [TestMethod]
    public void Enumerate_FilteredNullCollection_ReturnsEmpty()
    {
        IEnumerable<double>? values = null;
        Func<(int, double Value), bool> filter = t => t.Value % 2.0 == 0.0;

        (int, double)[] expected = [];
        (int, double)[] actual = [.. values.Enumerate(filter)];

        CollectionAssert.AreEquivalent(expected, actual, "Unexpected results.");
    }
#endregion // MethodTests
}
