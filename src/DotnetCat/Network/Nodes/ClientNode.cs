using System;
using System.IO;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes;

/// <summary>
///  TCP network socket client node.
/// </summary>
internal class ClientNode : Node
{
    private HostEndPoint? _targetEP;  // Remote target

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ClientNode() : base() => _targetEP = null;

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ClientNode(CmdLineArgs args) : base(args) => _targetEP = null;

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~ClientNode() => Dispose(false);

    /// <summary>
    ///  Establish a socket connection to the underlying IPv4 endpoint.
    /// </summary>
    public override void Connect()
    {
        ThrowIf.Null(Address);
        _targetEP = new HostEndPoint(HostName, Port);

        ValidateArgsCombinations();

        try  // Connect with 3.5-second timeout
        {
            if (!Client.ConnectAsync(Address, Port).Wait(3500))
            {
                throw Net.MakeException(SocketError.TimedOut);
            }
            NetStream = Client.GetStream();

            // Start the executable process
            if (Args.UsingExe && !StartProcess(ExePath))
            {
                PipeError(Except.ExeProcess, ExePath);
            }

            if (Args.PipeVariant is not PipeType.Status)
            {
                Output.Log($"Connected to {_targetEP}");
            }

            base.Connect();
            WaitForExit();

            Output.Log($"Connection to {_targetEP} closed");
        }
        catch (AggregateException ex)  // Asynchronous socket error occurred
        {
            PipeError(Net.GetExcept(ex), _targetEP, ex);
        }
        catch (SocketException ex)     // Socket error occurred
        {
            PipeError(Net.GetExcept(ex), _targetEP, ex);
        }
        catch (IOException ex)         // Connection was reset
        {
            PipeError(Except.ConnectionReset, Address.ToString(), ex);
        }

        Dispose();
    }
}
