using System.Diagnostics.CodeAnalysis;
using System.Net;
using DotnetCat.Errors;

namespace DotnetCat.Network;

/// <summary>
///  IPv4 hostname socket endpoint.
/// </summary>
internal class HostEndPoint
{
    private int _port;           // Network port number

    private string? _hostName;   // Network hostname

    private IPAddress _address;  // IPv4 address

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint()
    {
        _port = 44444;
        _address = IPAddress.Any;
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
        get => _port;
        set
        {
            ThrowIf.InvalidPort(value);
            _port = value;
        }
    }

    /// <summary>
    ///  Network hostname.
    /// </summary>
    public string HostName
    {
        get => _hostName ?? Address.ToString();
        set => _hostName = value;
    }

    /// <summary>
    ///  IPv4 address.
    /// </summary>
    public IPAddress Address
    {
        get => _address;
        set
        {
            ThrowIf.NotIPv4Address(value);
            _address = value;
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
