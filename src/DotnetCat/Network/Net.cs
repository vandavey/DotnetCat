using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.Shell;
using DotnetCat.Utils;

namespace DotnetCat.Network;

/// <summary>
///  Network and socket utility class.
/// </summary>
internal static class Net
{
    /// <summary>
    ///  Determine whether the given port is a valid network port number.
    /// </summary>
    public static bool ValidPort(int port) => port is > 0 and <= 65535;

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
        if (dnsAns is not null && !hostName.NoCaseEquals(SysInfo.Hostname))
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
    ///  Initialize a new socket exception from the given socket error.
    /// </summary>
    public static SocketException MakeException(SocketError error)
    {
        return new SocketException((int)error);
    }

    /// <summary>
    ///  Get the exception enum member associated to the given aggregate exception.
    /// </summary>
    public static Except GetExcept(AggregateException ex)
    {
        return GetExcept(SocketException(ex));
    }

    /// <summary>
    ///  Get the exception enum member associated to the given socket exception.
    /// </summary>
    public static Except GetExcept(SocketException? ex)
    {
        return ex?.SocketErrorCode switch
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
    }

    /// <summary>
    ///  Get the first socket exception nested within the given aggregate exception.
    /// </summary>
    public static SocketException? SocketException(AggregateException ex)
    {
        return ex.InnerExceptions.FirstOrDefaultOfType<SocketException>();
    }

    /// <summary>
    ///  Get all the IPv4 addresses from the given addresses.
    /// </summary>
    private static IEnumerable<IPAddress> IPv4Addresses(IEnumerable<IPAddress> addresses)
    {
        return addresses.Where(a => a.AddressFamily is AddressFamily.InterNetwork);
    }

    /// <summary>
    ///  Get the currently active local IPv4 address.
    /// </summary>
    private static IPAddress ActiveAddress()
    {
        using Socket socket = new(AddressFamily.InterNetwork,
                                  SocketType.Dgram,
                                  ProtocolType.Udp);

        socket.Connect("8.8.8.8", 53);

        return (socket.LocalEndPoint as IPEndPoint)?.Address ?? IPAddress.Any;
    }
}
