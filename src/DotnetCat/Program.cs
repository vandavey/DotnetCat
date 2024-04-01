using System;
using DotnetCat.Network.Nodes;
using DotnetCat.Utils;

namespace DotnetCat;

/// <summary>
///  Primary application startup object.
/// </summary>
internal class Program
{
    private static readonly Parser _parser;  // Command-line argument parser

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Program() => _parser = new Parser();

    /// <summary>
    ///  Network socket node.
    /// </summary>
    public static Node? SockNode { get; private set; }

    /// <summary>
    ///  Static application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        Console.Title = $"DotnetCat ({Parser.Repo})";

        // Display help information and exit
        if (args.IsNullOrEmpty() || Parser.NeedsHelp(args))
        {
            _parser.PrintHelp();
        }

        SockNode = Node.NewNode(_parser.Parse(args));
        SockNode.Connect();

        Console.WriteLine();
        Environment.Exit(0);
    }
}
