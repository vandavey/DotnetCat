using System;
using DotnetCat.Network.Nodes;
using DotnetCat.Utils;

namespace DotnetCat;

/// <summary>
///  Application startup object.
/// </summary>
internal class Program
{
    /// <summary>
    ///  Network socket node.
    /// </summary>
    public static Node? SockNode { get; private set; }

    /// <summary>
    ///  Application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        Console.Title = $"DotnetCat ({Parser.Repo})";
        Parser parser = new(args);

        // Display help information and exit
        if (parser.CmdArgs.Help)
        {
            Parser.PrintHelp();
        }

        SockNode = Node.NewNode(parser.CmdArgs);
        SockNode.Connect();

        Console.WriteLine();
        Environment.Exit(0);
    }
}
