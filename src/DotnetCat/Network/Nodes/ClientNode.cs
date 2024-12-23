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
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ClientNode(CmdLineArgs args) : base(args)
    {
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~ClientNode() => Dispose(false);

    /// <summary>
    ///  Establish a socket connection to the underlying IPv4 endpoint.
    /// </summary>
    public override void Connect()
    {
        ValidateArgsCombinations();

        try  // Connect to the remote endpoint
        {
            if (!Client.ConnectAsync(Endpoint.IPv4Endpoint()).Wait(3500))
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
                Output.Log($"Connected to {Endpoint}");
            }

            base.Connect();
            WaitForExit();

            Output.Log($"Connection to {Endpoint} closed");
        }
        catch (AggregateException ex)
        {
            PipeError(Net.GetExcept(ex), Endpoint, ex);
        }
        catch (SocketException ex)
        {
            PipeError(Net.GetExcept(ex), Endpoint, ex);
        }
        catch (IOException ex)
        {
            PipeError(Except.ConnectionReset, Endpoint, ex);
        }

        Dispose();
    }
}
