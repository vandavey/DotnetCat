using System;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Errors;
using DotnetCat.Network;

namespace DotnetCatTests.Network;

/// <summary>
///  Unit tests for utility class <c>DotnetCat.Network.Net</c>.
/// </summary>
[TestClass]
public class NetTests
{
    /// <summary>
    ///  Assert that an input <c>AggregateException</c> returns the
    ///  expected <c>Except</c> enumeration type member.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.SocketError, (byte)Except.SocketError)]
    [DataRow(SocketError.ConnectionRefused, (byte)Except.ConnectionRefused)]
    [DataRow(SocketError.SystemNotReady, (byte)Except.SocketError)]
    public void GetExcept_AggregateException_ReturnsExpected(SocketError error,
                                                             byte expectedByte) {
        SocketException innerEx = new((int)error);
        AggregateException aggregateEx = new(innerEx);

        Except expected = (Except)expectedByte;
        Except actual = Net.GetExcept(aggregateEx);

        Assert.AreEqual(actual, expected);
    }

    /// <summary>
    ///  Assert that an input <c>SocketException</c> returns the
    ///  expected <c>Except</c> enumeration type member.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.SocketError, (byte)Except.SocketError)]
    [DataRow(SocketError.ConnectionRefused, (byte)Except.ConnectionRefused)]
    [DataRow(SocketError.SystemNotReady, (byte)Except.SocketError)]
    public void GetExcept_SocketException_ReturnsExpected(SocketError error,
                                                          byte expectedByte) {
        SocketException socketEx = new((int)error);

        Except expected = (Except)expectedByte;
        Except actual = Net.GetExcept(socketEx);

        Assert.AreEqual(actual, expected);
    }

    /// <summary>
    ///  Assert that a valid input network port number returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow(80)]
    [DataRow(443)]
    [DataRow(8443)]
    public void IsValidPort_ValidPort_ReturnsTrue(int port)
    {
        bool actual = Net.IsValidPort(port);
        Assert.IsTrue(actual);
    }

    /// <summary>
    ///  Assert that an invalid input network port number returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow(-80)]
    [DataRow(0)]
    [DataRow(65536)]
    public void IsValidPort_InvalidPort_ReturnsFalse(int port)
    {
        bool actual = Net.IsValidPort(port);
        Assert.IsFalse(actual);
    }

    /// <summary>
    ///  Assert that an input socket error returns a <c>SocketException</c>
    ///  that was constructed with the correct socket error.
    /// </summary>
    [DataTestMethod]
    [DataRow(SocketError.HostDown)]
    [DataRow(SocketError.NetworkUnreachable)]
    [DataRow(SocketError.TimedOut)]
    public void MakeException_Error_ReturnsWithCorrectError(SocketError error)
    {
        SocketException actual = Net.MakeException(error);
        Assert.AreEqual(actual.SocketErrorCode, error);
    }
}
