using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using DotnetCat.Shell;
using DotnetCat.Utils;

namespace DotnetCat.Network;

/// <summary>
///  Socket timeout exception.
/// </summary>
internal sealed class SocketTimeoutException : SocketException
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public SocketTimeoutException() : base((int)SocketError.TimedOut)
    {
        if (base.StackTrace.IsNullOrEmpty())
        {
            IEnumerable<StackFrame> frames = new StackTrace(1).GetFrames().Take(4);
            StackTrace = new StackTrace(frames).ToString();

            if (StackTrace.EndsWith(SysInfo.Eol))
            {
                StackTrace = StackTrace[..^SysInfo.Eol.Length];
            }
        }
        else  // Use default stack trace
        {
            StackTrace = base.StackTrace;
        }
    }

    /// <summary>
    ///  Exception stack trace.
    /// </summary>
    public override string? StackTrace { get; }
}
