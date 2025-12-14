using System.Diagnostics.CodeAnalysis;
using System.Net;
using DotnetCat.Errors;

namespace DotnetCat.Network;

/// <summary>
///  IPv4 hostname socket endpoint.
/// </summary>
internal class HostEndPoint
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint()
    {
        Port = 44444;
        Address = IPAddress.Any;
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint([NotNull] string? hostName, IPAddress? address, int port) : this()
    {
        ThrowIf.NullOrEmpty(hostName);
        ThrowIf.Null(address);

        Port = port;
        HostName = hostName;
        Address = address;
    }

    /// <summary>
    ///  Network port number.
    /// </summary>
    public int Port
    {
        get => field;
        set
        {
            ThrowIf.InvalidPort(value);
            field = value;
        }
    }

    /// <summary>
    ///  Network hostname.
    /// </summary>
    public string HostName
    {
        get => field ?? Address.ToString();
        set;
    }

    /// <summary>
    ///  IPv4 address.
    /// </summary>
    public IPAddress Address
    {
        get => field;
        set
        {
            ThrowIf.NotIPv4Address(value);
            field = value;
        }
    }

    /// <summary>
    ///  Get the string representation of the underlying endpoint information.
    /// </summary>
    public override string? ToString() => $"{HostName}:{Port}";

    /// <summary>
    ///  Parse the given IPv4 endpoint.
    /// </summary>
    public void ParseEndpoint([NotNull] IPEndPoint? ipEndpoint)
    {
        ThrowIf.Null(ipEndpoint);
        Address = ipEndpoint.Address;
        Port = ipEndpoint.Port;
    }

    /// <summary>
    ///  Initialize an IPv4 endpoint from the underlying IP address and port number.
    /// </summary>
    public IPEndPoint IPv4Endpoint() => new(Address, Port);
}
