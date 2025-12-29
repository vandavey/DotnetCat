using System;
using System.Diagnostics.CodeAnalysis;
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
            _listener.Listen(1);
            Output.Log($"Listening for incoming connections on {Endpoint}...");

            Socket = _listener.Accept();
            NetStream = new NetworkStream(Socket, ownsSocket: false);

            if (Args.UsingExe && !StartProcess(ExePath))
            {
                PipeError(Except.ExeProcess, ExePath);
            }

            remoteEndpoint.ParseEndpoint(Socket.RemoteEndPoint as IPEndPoint);
            Output.Log($"Connected to {remoteEndpoint}");

            base.Connect();
            WaitForExit();

            Console.WriteLine();
            Output.Log($"Connection to {remoteEndpoint} closed");
        }
        catch (Exception ex) when (ex is AggregateException or SocketException)
        {
            PipeError(Net.GetExcept(ex), remoteEndpoint, ex);
        }
        catch (IOException ex)
        {
            PipeError(Except.ConnectionReset, remoteEndpoint, ex);
        }
        finally
        {
            Dispose();
        }
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
    [MemberNotNull(nameof(_listener))]
    private void BindListener(IPEndPoint ep)
    {
        _listener = Net.MakeSocket(ProtocolType.Tcp);

        try  // Bind listener socket to endpoint
        {
            _listener.Bind(ep);
        }
        catch (SocketException ex)
        {
            PipeError(Net.GetExcept(ex), ep.ToString(), ex);
        }
    }
}
