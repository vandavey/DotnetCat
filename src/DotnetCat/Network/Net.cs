using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.Shell;
using DotnetCat.Utils;
using static DotnetCat.Network.Constants;

namespace DotnetCat.Network;

/// <summary>
///  Network and socket utility class.
/// </summary>
internal static class Net
{
    /// <summary>
    ///  Determine whether the given port is a valid network port number.
    /// </summary>
    public static bool ValidPort(int port) => port is >= MIN_PORT and <= MAX_PORT;

    /// <summary>
    ///  Get the exception enumerator associated with the given exception.
    /// </summary>
    public static Except GetExcept<T>(T ex) where T : Exception
    {
        ThrowIf.TypeMismatch<T, (AggregateException, SocketException)>(ex);
        Except except = default;

        if (ex is AggregateException aggregateEx)
        {
            except = GetExcept(aggregateEx);
        }
        else if (ex is SocketException socketEx)
        {
            except = GetExcept(socketEx);
        }
        return except;
    }

    /// <summary>
    ///  Get the exception enumerator associated with the given aggregate exception.
    /// </summary>
    public static Except GetExcept(AggregateException ex)
    {
        return GetExcept(SocketException(ex));
    }

    /// <summary>
    ///  Get the exception enumerator associated with the given socket exception.
    /// </summary>
    public static Except GetExcept(SocketException? ex) => ex?.SocketErrorCode switch
    {
        SocketError.AddressAlreadyInUse => Except.AddressInUse,
        SocketError.NetworkDown         => Except.NetworkDown,
        SocketError.NetworkUnreachable  => Except.NetworkUnreachable,
        SocketError.NetworkReset        => Except.NetworkReset,
        SocketError.ConnectionAborted   => Except.ConnectionAborted,
        SocketError.ConnectionReset     => Except.ConnectionReset,
        SocketError.TimedOut            => Except.TimedOut,
        SocketError.ConnectionRefused   => Except.ConnectionRefused,
        SocketError.HostUnreachable     => Except.HostUnreachable,
        SocketError.HostNotFound        => Except.HostNotFound,
        SocketError.SocketError or _    => Except.SocketError
    };

    /// <summary>
    ///  Resolve the IPv4 address associated with the given hostname.
    /// </summary>
    public static IPAddress ResolveName(string hostName, out Exception? ex)
    {
        ex = null;
        IPHostEntry? dnsAns = null;

        try  // Resolve IPv4 from hostname
        {
            dnsAns = Dns.GetHostEntry(hostName);
        }
        catch (SocketException sockEx)
        {
            ex = sockEx;
        }

        IPAddress address = IPAddress.None;

        // Extract first resulting IPv4 address
        if (dnsAns is not null && !hostName.IgnCaseEquals(SysInfo.Hostname))
        {
            IPAddress? addr = IPv4Addresses(dnsAns.AddressList).FirstOrDefault();

            if (addr is not null)
            {
                address = addr;
            }
            else  // Name resolution failure
            {
                ex = MakeException(SocketError.HostNotFound);
            }
        }

        return ex is null && address.Equals(IPAddress.None) ? ActiveAddress() : address;
    }

    /// <summary>
    ///  Initialize a socket exception from the given socket error.
    /// </summary>
    public static SocketException MakeException(SocketError error)
    {
        return new SocketException((int)error);
    }

    /// <summary>
    ///  Get the first socket exception nested within the given aggregate exception.
    /// </summary>
    public static SocketException? SocketException(AggregateException ex)
    {
        return ex.InnerExceptions.FirstOrDefaultOfType<SocketException>();
    }

    /// <summary>
    ///  Create a socket for network communications using the given protocol type.
    /// </summary>
    public static Socket MakeSocket(ProtocolType protocol)
    {
        Socket socket;

        if (ThrowIf.InvalidProtocol(protocol) is ProtocolType.Tcp)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocol);
        }
        else  // Create UDP socket
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, protocol);
        }
        return socket;
    }

    /// <summary>
    ///  Get the currently active local IPv4 address.
    /// </summary>
    private static IPAddress ActiveAddress()
    {
        using Socket socket = MakeSocket(ProtocolType.Udp);
        socket.Connect("8.8.8.8", 53);

        return (socket.LocalEndPoint as IPEndPoint)?.Address ?? IPAddress.Any;
    }

    /// <summary>
    ///  Get all the IPv4 addresses from the given address collection.
    /// </summary>
    private static IEnumerable<IPAddress> IPv4Addresses(IEnumerable<IPAddress> addresses)
    {
        return addresses.Where(a => a.AddressFamily is AddressFamily.InterNetwork);
    }
}
