using System;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Errors;
using DotnetCat.Network;

namespace DotnetCatTests.Network;

/// <summary>
///  Unit tests for utility class <see cref="Net"/>.
/// </summary>
[TestClass]
public class NetTests
{
#region MethodTests
    /// <summary>
    ///  Assert that an input <see cref="AggregateException"/> returns
    ///  the expected <see cref="Except"/> enumeration type member.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.SocketError, (byte)Except.SocketError)]
    [DataRow(SocketError.ConnectionRefused, (byte)Except.ConnectionRefused)]
    [DataRow(SocketError.SystemNotReady, (byte)Except.SocketError)]
    public void GetExcept_AggregateException_ReturnsExpected(SocketError error,
                                                             byte expectedByte)
    {
        SocketException innerEx = new((int)error);
        AggregateException aggregateEx = new(innerEx);

        Except expected = (Except)expectedByte;
        Except actual = Net.GetExcept(aggregateEx);

        Assert.AreEqual(actual, expected, $"Enum result should be '{expected}'");
    }

    /// <summary>
    ///  Assert that an input <see cref="SocketException"/> returns
    ///  the expected <see cref="Except"/> enumeration type member.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.SocketError, (byte)Except.SocketError)]
    [DataRow(SocketError.ConnectionRefused, (byte)Except.ConnectionRefused)]
    [DataRow(SocketError.SystemNotReady, (byte)Except.SocketError)]
    public void GetExcept_SocketException_ReturnsExpected(SocketError error,
                                                          byte expectedByte)
    {
        SocketException socketEx = new((int)error);

        Except expected = (Except)expectedByte;
        Except actual = Net.GetExcept(socketEx);

        Assert.AreEqual(actual, expected, $"Enum result should be '{expected}'");
    }

    /// <summary>
    ///  Assert that an input <see cref="AggregateException"/> with an inner
    ///  <see cref="SocketException"/> returns the inner <see cref="SocketException"/>.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.SocketError)]
    [DataRow(SocketError.ConnectionRefused)]
    [DataRow(SocketError.SystemNotReady)]
    public void GetException_InnerException_ReturnsException(SocketError error)
    {
        SocketException expected = new((int)error);
        AggregateException aggregateEx = new(expected);

        SocketException? actual = Net.GetException(aggregateEx);

        Assert.AreEqual(actual, expected, "Failure extracting socket exception");
    }

    /// <summary>
    ///  Assert that an input <see cref="AggregateException"/> without
    ///  an inner <see cref="SocketException"/> returns null.
    /// </summary>
    [TestMethod]
    public void GetException_NoInnerException_ReturnsNull()
    {
        AggregateException aggregateEx = new();
        SocketException? actual = Net.GetException(aggregateEx);

        Assert.IsNull(actual, "Resulting socket exception should be null");
    }

    /// <summary>
    ///  Assert that a valid input network port number returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow(80)]
    [DataRow(443)]
    [DataRow(8443)]
    public void ValidPort_ValidPort_ReturnsTrue(int port)
    {
        bool actual = Net.ValidPort(port);
        Assert.IsTrue(actual, $"Port '{port}' should be considered valid");
    }

    /// <summary>
    ///  Assert that an invalid input network port number returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(-80)]
    [DataRow(0)]
    [DataRow(65536)]
    public void ValidPort_InvalidPort_ReturnsFalse(int port)
    {
        bool actual = Net.ValidPort(port);
        Assert.IsFalse(actual, $"Port '{port}' should be considered invalid");
    }

    /// <summary>
    ///  Assert that an input socket error returns a <see cref="SocketException"/>
    ///  that was constructed with the correct socket error.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.HostDown)]
    [DataRow(SocketError.NetworkUnreachable)]
    [DataRow(SocketError.TimedOut)]
    public void MakeException_Error_ReturnsWithCorrectError(SocketError expected)
    {
        SocketException socketEx = Net.MakeException(expected);
        SocketError actual = socketEx.SocketErrorCode;

        Assert.AreEqual(actual, expected, $"Expected error code: '{expected}'");
    }
#endregion // MethodTests
}
