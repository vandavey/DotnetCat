using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes;

/// <summary>
///  TCP network socket server node.
/// </summary>
internal class ServerNode : Node
{
    private bool _disposed;     // Object disposed

    private Socket? _listener;  // Listener socket

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ServerNode(CmdLineArgs args) : base(args) => _disposed = false;

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~ServerNode() => Dispose(false);

    /// <summary>
    ///  Listen for an inbound TCP connection on the underlying listener socket.
    /// </summary>
    public override void Connect()
    {
        ValidateArgsCombinations();

        HostEndPoint remoteEndpoint = new();
        BindListener(Endpoint.IPv4Endpoint());

        try  // Listen for an inbound connection
        {
            _listener?.Listen(1);
            Output.Log($"Listening for incoming connections on {Endpoint}...");

            if (_listener is not null)
            {
                Client.Client = _listener.Accept();
            }
            NetStream = Client.GetStream();

            if (Args.UsingExe && !StartProcess(ExePath))
            {
                PipeError(Except.ExeProcess, ExePath);
            }

            remoteEndpoint.ParseEndpoint(Client.Client.RemoteEndPoint as IPEndPoint);
            Output.Log($"Connected to {remoteEndpoint}");

            base.Connect();
            WaitForExit();

            Console.WriteLine();
            Output.Log($"Connection to {remoteEndpoint} closed");
        }
        catch (AggregateException ex)
        {
            PipeError(Net.GetExcept(ex), remoteEndpoint, ex);
        }
        catch (SocketException ex)
        {
            PipeError(Net.GetExcept(ex), remoteEndpoint, ex);
        }
        catch (IOException ex)
        {
            PipeError(Except.ConnectionReset, remoteEndpoint, ex);
        }

        Dispose();
    }

    /// <summary>
    ///  Free the underlying resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _listener?.Close();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    /// <summary>
    ///  Bind the underlying listener socket to the given IPv4 endpoint.
    /// </summary>
    private void BindListener(IPEndPoint ep)
    {
        _listener = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream,
                               ProtocolType.Tcp);

        try  // Bind the listener socket
        {
            _listener.Bind(ep);
        }
        catch (SocketException ex)
        {
            PipeError(Net.GetExcept(ex), ep.ToString(), ex);
        }
    }
}
