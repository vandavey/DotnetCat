using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Errors;

namespace DotnetCat.Network;

/// <summary>
///  Network and socket utility class.
/// </summary>
internal static class Net
{
    /// <summary>
    ///  Determine whether the given port is a valid network port number.
    /// </summary>
    public static bool IsValidPort(int port) => port is > 0 and <= 65535;

    /// <summary>
    ///  Resolve the IPv4 address associated with the given hostname.
    /// </summary>
    public static (IPAddress ip, Exception? ex) ResolveName(string hostName)
    {
        IPHostEntry dnsAns;
        IPAddress address = IPAddress.Any;

        try  // Resolve IPv4 from hostname
        {
            dnsAns = Dns.GetHostEntry(hostName);

            if (dnsAns.AddressList.Contains(IPAddress.Loopback))
            {
                return (IPAddress.Loopback, null);
            }
        }
        catch (SocketException ex)  // No DNS entries found
        {
            return (address, ex);
        }

        // Return the first IPv4 address
        if (dnsAns.HostName.ToLower() != Environment.MachineName.ToLower())
        {
            foreach (IPAddress addr in dnsAns.AddressList)
            {
                if (addr.AddressFamily is AddressFamily.InterNetwork)
                {
                    return (addr, null);
                }
            }
            return (address, GetSocketException(SocketError.HostNotFound));
        }

        return (ActiveLocalAddress(), null);
    }

    /// <summary>
    ///  Get a new socket exception initialized from the given socket error.
    /// </summary>
    public static SocketException GetSocketException(SocketError error)
    {
        return new SocketException(Convert.ToInt32(error));
    }

    /// <summary>
    ///  Get the DotnetCat exception associated with the given socket exception.
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
    ///  Get the DotnetCat exception associated with the given aggregate exception.
    /// </summary>
    public static Except GetExcept(AggregateException ex)
    {
        Exception? exception = (from Exception innerEx in ex.InnerExceptions
                                where innerEx is SocketException
                                select innerEx).FirstOrDefault();

        return GetExcept(exception as SocketException);
    }

    /// <summary>
    ///  Get the currently active local IPv4 address.
    /// </summary>
    private static IPAddress ActiveLocalAddress()
    {
        using Socket socket = new(AddressFamily.InterNetwork,
                                  SocketType.Dgram,
                                  ProtocolType.Udp);

        socket.Connect("8.8.8.8", 53);

        return (socket?.LocalEndPoint as IPEndPoint)?.Address ?? IPAddress.Any;
    }
}
