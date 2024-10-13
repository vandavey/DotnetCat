using System.Net;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.Network;

/// <summary>
///  IPv4 hostname socket endpoint.
/// </summary>
internal class HostEndPoint
{
    private int _port;          // Network port number

    private string? _hostName;  // Network hostname

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint() => _port = -1;

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint(string? hostName, int port)
    {
        HostName = hostName;
        Port = port;
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint(IPAddress address, int port)
    {
        HostName = address.ToString();
        Port = port;
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public HostEndPoint(IPEndPoint? ep)
    {
        ThrowIf.Null(ep);

        HostName = ep.Address.ToString();
        Port = ep.Port;
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
    public string? HostName
    {
        get => _hostName;
        set
        {
            ThrowIf.NullOrEmpty(value);
            _hostName = value;
        }
    }

    /// <summary>
    ///  Get the string representation of the underlying endpoint information.
    /// </summary>
    public override string? ToString()
    {
        string? endPointStr;

        if (Port <= -1 || HostName.IsNullOrEmpty())
        {
            endPointStr = base.ToString();
        }
        else
        {
            endPointStr = $"{HostName}:{Port}";
        }
        return endPointStr;
    }
}
