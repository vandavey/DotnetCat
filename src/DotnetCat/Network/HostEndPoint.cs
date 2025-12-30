using System.Diagnostics.CodeAnalysis;
using System.Net;
using DotnetCat.Errors;
using static DotnetCat.Network.Constants;

namespace DotnetCat.Network;

/// <summary>
///  IPv4 hostname socket endpoint.
/// </summary>
internal sealed class HostEndPoint
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint()
    {
        Port = DEFAULT_PORT;
        Address = IPAddress.Any;
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint([NotNull] string? hostName,
                        [NotNull] IPAddress? address,
                        int port)
        : this()
    {
        Port = port;
        HostName = ThrowIf.NullOrEmpty(hostName);
        Address = ThrowIf.Null(address);
    }

    /// <summary>
    ///  Network port number.
    /// </summary>
    public int Port
    {
        get;
        set => field = ThrowIf.InvalidPort(value);
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
        get;
        set => field = ThrowIf.NotIPv4Address(value);
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
        Address = ThrowIf.Null(ipEndpoint).Address;
        Port = ipEndpoint.Port;
    }

    /// <summary>
    ///  Initialize an IPv4 endpoint from the underlying IP address and port number.
    /// </summary>
    public IPEndPoint IPv4Endpoint() => new(Address, Port);
}
