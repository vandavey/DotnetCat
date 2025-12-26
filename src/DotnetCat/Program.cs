using System;
using DotnetCat.Network.Nodes;
using DotnetCat.Utils;
using static DotnetCat.Utils.Constants;

namespace DotnetCat;

/// <summary>
///  Application startup object.
/// </summary>
internal class Program
{
    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Program() => Console.Title = $"DotnetCat ({REPO_URL})";

    /// <summary>
    ///  Network socket node.
    /// </summary>
    public static Node? SockNode { get; private set; }

    /// <summary>
    ///  Application entry point.
    /// </summary>
    public static int Main(string[] args)
    {
        Parser parser = new(args);

        // Display help information and exit
        if (parser.CmdArgs.Help)
        {
            Parser.PrintHelp();
        }

        SockNode = Node.Make(parser.CmdArgs);
        SockNode.Connect();

        Console.WriteLine();
        return NO_ERROR_EXIT_CODE;
    }
}
