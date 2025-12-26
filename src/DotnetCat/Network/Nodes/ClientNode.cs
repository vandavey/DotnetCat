using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Utils;
using static DotnetCat.Network.Constants;

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
        Socket = Net.MakeSocket(ProtocolType.Tcp);
    }

    /// <summary>
    ///  Finalize the object.
    /// </summary>
    ~ClientNode() => Dispose(false);

    /// <summary>
    ///  Establish a socket connection to the underlying IPv4 endpoint.
    /// </summary>
    public override void Connect() => ConnectAsync().AwaitResult();

    /// <summary>
    ///  Asynchronously establish a socket connection to the underlying IPv4 endpoint.
    /// </summary>
    private async Task ConnectAsync()
    {
        ThrowIf.Null(Socket);
        ValidateArgsCombinations();

        try  // Connect to the remote endpoint
        {
            using CancellationTokenSource tokenSrc = new(CONNECT_TIMEOUT);
            await Socket.ConnectAsync(Endpoint.IPv4Endpoint(), tokenSrc.Token);

            NetStream = new NetworkStream(Socket, ownsSocket: false);

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
            await WaitForExitAsync();

            Output.Log($"Connection to {Endpoint} closed");
        }
        catch (Exception ex) when (ex is AggregateException or SocketException)
        {
            PipeError(Net.GetExcept(ex), Endpoint, ex);
        }
        catch (OperationCanceledException)
        {
            PipeError(Except.TimedOut, Endpoint, new SocketTimeoutException());
        }
        catch (IOException ex)
        {
            PipeError(Except.ConnectionReset, Endpoint, ex);
        }
        finally
        {
            Dispose();
        }
    }
}
