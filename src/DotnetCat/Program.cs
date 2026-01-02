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

        using Node socketNode = Node.Make(parser.CmdArgs);
        socketNode.Connect();

        Console.WriteLine();
        return NO_ERROR_EXIT_CODE;
    }
}
