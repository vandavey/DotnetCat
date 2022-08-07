using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DotnetCat.Network
{
    /// <summary>
    ///  Network information controller
    /// </summary>
    internal static class Net
    {
        /// <summary>
        ///  Determine if the given port number is valid
        /// </summary>
        public static bool IsValidPort(int port) => port is > 0 and <= 65535;

        /// <summary>
        ///  Resolve the IPv4 address of given host name
        /// </summary>
        public static (IPAddress ip, Exception? ex) ResolveName(string hostName)
        {
            IPHostEntry dnsAns;
            IPAddress ipAddress = IPAddress.Any;

            string machineName = Environment.MachineName;

            try  // Resolve host name as IP address
            {
                dnsAns = Dns.GetHostEntry(hostName);

                if (dnsAns.AddressList.Contains(IPAddress.Loopback))
                {
                    return (IPAddress.Loopback, null);
                }
            }
            catch (SocketException ex)  // No DNS entries found
            {
                return (ipAddress, ex);
            }

            if (dnsAns.HostName.ToLower() != machineName.ToLower())
            {
                foreach (IPAddress addr in dnsAns.AddressList)
                {
                    // Return the first IPv4 address
                    if (addr.AddressFamily is AddressFamily.InterNetwork)
                    {
                        return (addr, null);
                    }
                }
                return (ipAddress, new SocketException(11001));
            }

            using Socket socket = new(AddressFamily.InterNetwork,
                                      SocketType.Dgram,
                                      ProtocolType.Udp);

            socket.Connect("8.8.8.8", 53);

            // Get the active local IP endpoint
            IPEndPoint? endPoint = socket?.LocalEndPoint as IPEndPoint;

            if (endPoint is not null)
            {
                ipAddress = endPoint.Address;
            }
            return (ipAddress, null);
        }
    }
}
