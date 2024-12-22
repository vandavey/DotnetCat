using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Network;
using DotnetCat.Shell;
using IndexedAlias = (int Index, string Alias);
using IndexedArg = (int Index, string Arg);
using IndexedFlag = (int Index, string Flag);

namespace DotnetCat.Utils;

/// <summary>
///  Command-line argument parser and validator.
/// </summary>
internal partial class Parser
{
    private static readonly string _title;         // Application title

    private readonly List<int> _processedIndexes;  // Processed argument indexes

    private List<string> _argsList;                // Command-line argument list

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Parser() => _title = SysInfo.IsLinux() ? "dncat" : "dncat.exe";

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public Parser()
    {
        _processedIndexes = [];
        _argsList = [];

        CmdArgs = new CmdLineArgs();
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public Parser(IEnumerable<string> args) : this() => Parse(args);

    /// <summary>
    ///  Application repository URL.
    /// </summary>
    public static string Repo => "https://github.com/vandavey/DotnetCat";

    /// <summary>
    ///  Application usage string.
    /// </summary>
    public static string Usage => $"Usage: {_title} [OPTIONS] TARGET";

    /// <summary>
    ///  Parsed command-line arguments.
    /// </summary>
    public CmdLineArgs CmdArgs { get; }

    /// <summary>
    ///  Write the extended application usage information to the
    ///  standard console output stream and exit the application.
    /// </summary>
    [DoesNotReturn]
    public static void PrintHelp()
    {
        Console.WriteLine(GetHelpMessage());
        Environment.Exit(0);
    }

    /// <summary>
    ///  Parse and validate all the arguments in
    ///  the given command-line argument collection.
    /// </summary>
    public CmdLineArgs Parse(IEnumerable<string> args)
    {
        _argsList = DefragArguments([.. args]);
        CmdArgs.Help = HelpFlagParsed();

        if (!CmdArgs.Help)
        {
            HandleMalformedArgs();
            ParseCharArgs();
            ParseFlagArgs();
            ParsePositionalArgs();
        }
        return CmdArgs;
    }

    /// <summary>
    ///  Defragment the given fragmented command-line arguments
    ///  so quoted strings are interpreted as single arguments.
    /// </summary>
    private static List<string> DefragArguments(List<string> args)
    {
        List<string> defraggedArgs = [];

        // Defragment the given arguments
        for (int i = 0; i < args.Count; i++)
        {
            bool begQuoted = args[i].StartsWithQuote();

            if (!begQuoted || (begQuoted && args[i].EndsWithQuote()))
            {
                defraggedArgs.Add(args[i]);
                continue;
            }

            if (i == args.Count - 1)
            {
                defraggedArgs.Add(args[i]);
                break;
            }

            // Locate terminating argument and parse the range
            for (int j = i + 1; j < args.Count; j++)
            {
                if (args[j].EndsWithQuote())
                {
                    string argStr = args[i..(j + 1)].Join(" ");

                    // Remove leading and trailing quotes
                    if (argStr.Length >= 2)
                    {
                        argStr = argStr[1..^1];
                    }
                    defraggedArgs.Add(argStr);

                    i = j;
                    break;
                }
            }
        }

        return defraggedArgs;
    }

    /// <summary>
    ///  Determine whether the given argument is a command-line flag alias
    ///  argument. Flag alias arguments begin with one dash (e.g., <c>-f</c>).
    /// </summary>
    private static bool IsAlias(string arg) => AliasRegex().IsMatch(arg);

    /// <summary>
    ///  Determine whether the argument in the given tuple is a command-line flag
    ///  alias argument. Flag alias arguments begin with one dash (e.g., <c>-f</c>).
    /// </summary>
    private static bool IsAlias(IndexedArg idxArg) => IsAlias(idxArg.Arg);

    /// <summary>
    ///  Determine whether the given argument is a command-line flag
    ///  argument. Flag arguments begin with two dashes (e.g., <c>--foo</c>).
    /// </summary>
    private static bool IsFlag(string arg) => FlagRegex().IsMatch(arg);

    /// <summary>
    ///  Determine whether the argument in the given tuple is a command-line
    ///  flag argument. Flag arguments begin with two dashes (e.g., <c>--foo</c>).
    /// </summary>
    private static bool IsFlag(IndexedArg idxArg) => IsFlag(idxArg.Arg);

    /// <summary>
    ///  Get the extended application usage information message.
    /// </summary>
    private static string GetHelpMessage()
    {
        string helpMessage = $"""
            DotnetCat ({Repo})
            {Usage}{SysInfo.Eol}
            Remote command shell application{SysInfo.Eol}
            Positional Arguments:
              TARGET                    Remote or local IPv4 address{SysInfo.Eol}
            Optional Arguments:
              -h/-?,   --help           Show this help message and exit
              -v,      --verbose        Enable verbose console output
              -d,      --debug          Output verbose error information
              -l,      --listen         Listen for incoming connections
              -z,      --zero-io        Report connection status only
              -p PORT, --port PORT      Specify port to use for endpoint.
                                        (Default: 44444)
              -e EXEC, --exec EXEC      Executable process file path
              -o PATH, --output PATH    Receive file from remote host
              -s PATH, --send PATH      Send local file or folder
              -t DATA, --text DATA      Send string to remote host{SysInfo.Eol}
            Usage Examples:
              {_title} --listen --exec powershell.exe
              {_title} -d -p 44444 localhost
              {_title} -vo test.txt -p 2009 192.168.1.9{SysInfo.Eol}
            """;
        return helpMessage;
    }

    /// <summary>
    ///  Command-line flag alias argument regular expression.
    /// </summary>
    [GeneratedRegex(@"^-[?\w]+$")]
    private static partial Regex AliasRegex();

    /// <summary>
    ///  Command-line flag argument regular expression.
    /// </summary>
    [GeneratedRegex(@"^--\w+(-*\w*)*$")]
    private static partial Regex FlagRegex();

    /// <summary>
    ///  Command-line help flag or flag alias argument regular expression.
    /// </summary>
    [GeneratedRegex(@"^-(-help|\w*[?Hh]+\w*)$")]
    private static partial Regex HelpFlagRegex();

    /// <summary>
    ///  Determine whether any of the help flag (<c>-h</c>, <c>-?</c>, <c>--help</c>)
    ///  named arguments exist in the underlying command-line argument list.
    /// </summary>
    private bool HelpFlagParsed()
    {
        return _argsList.IsNullOrEmpty() || _argsList.Any(HelpFlagRegex().IsMatch);
    }

    /// <summary>
    ///  Handle malformed argument errors if any malformed arguments were parsed.
    /// </summary>
    private void HandleMalformedArgs()
    {
        if (_argsList.Contains("-"))
        {
            Error.Handle(Except.InvalidArgs, "-", true);
        }
        else if (_argsList.Contains("--"))
        {
            Error.Handle(Except.InvalidArgs, "--", true);
        }
    }

    /// <summary>
    ///  Parse the named flag alias arguments in the underlying command-line
    ///  argument list. Flag alias arguments begin with one dash (e.g., <c>-f</c>).
    /// </summary>
    private void ParseCharArgs()
    {
        foreach (IndexedAlias idxAlias in _argsList.Enumerate(IsAlias))
        {
            foreach (char ch in idxAlias.Alias)
            {
                switch (ch)
                {
                    case '-':
                        continue;
                    case 'l':
                        CmdArgs.Listen = true;
                        break;
                    case 'v':
                        CmdArgs.Verbose = true;
                        break;
                    case 'd':
                        CmdArgs.Debug = CmdArgs.Verbose = Error.Debug = true;
                        break;
                    case 'z':
                        CmdArgs.PipeVariant = PipeType.Status;
                        break;
                    case 'p':
                        ParsePort(idxAlias);
                        break;
                    case 'e':
                        ParseExecutable(idxAlias);
                        break;
                    case 't':
                        ParseTextPayload(idxAlias);
                        break;
                    case 'o':
                        ParseTransferPath(idxAlias, TransferOpt.Collect);
                        break;
                    case 's':
                        ParseTransferPath(idxAlias, TransferOpt.Transmit);
                        break;
                    default:
                        Error.Handle(Except.UnknownArgs, $"-{ch}", true);
                        break;
                }
            }
            AddProcessedArg(idxAlias);
        }

        RemoveProcessedArgs();
    }

    /// <summary>
    ///  Parse the named flag arguments in the underlying command-line argument
    ///  list. Flag arguments begin with two dashes (e.g., <c>--foo</c>).
    /// </summary>
    private void ParseFlagArgs()
    {
        foreach (IndexedFlag idxFlag in _argsList.Enumerate(IsFlag))
        {
            switch (idxFlag.Flag)
            {
                case "--listen":
                    CmdArgs.Listen = true;
                    break;
                case "--verbose":
                    CmdArgs.Verbose = true;
                    break;
                case "--debug":
                    CmdArgs.Debug = CmdArgs.Verbose = Error.Debug = true;
                    break;
                case "--zero-io":
                    CmdArgs.PipeVariant = PipeType.Status;
                    break;
                case "--port":
                    ParsePort(idxFlag);
                    break;
                case "--exec":
                    ParseExecutable(idxFlag);
                    break;
                case "--text":
                    ParseTextPayload(idxFlag);
                    break;
                case "--output":
                    ParseTransferPath(idxFlag, TransferOpt.Collect);
                    break;
                case "--send":
                    ParseTransferPath(idxFlag, TransferOpt.Transmit);
                    break;
                default:
                    Error.Handle(Except.UnknownArgs, idxFlag.Flag, true);
                    break;
            }
            AddProcessedArg(idxFlag);
        }

        RemoveProcessedArgs();
    }

    /// <summary>
    ///  Remove processed command-line arguments from the underlying argument list.
    /// </summary>
    private void RemoveProcessedArgs()
    {
        int delta = 0;

        _processedIndexes.Order().ForEach(i => _argsList.RemoveAt(i - delta++));
        _processedIndexes.Clear();
    }

    /// <summary>
    ///  Parse the positional arguments in the underlying command-line argument
    ///  list. Positional arguments do not begin with <c>-</c> or <c>--</c>.
    /// </summary>
    private void ParsePositionalArgs()
    {
        switch (_argsList.Count)
        {
            case 0:   // Missing TARGET
            {
                if (!CmdArgs.Listen)
                {
                    Error.Handle(Except.RequiredArgs, "TARGET", true);
                }
                break;
            }
            case 1:   // Validate TARGET
            {
                if (_argsList[0].StartsWith('-'))
                {
                    Error.Handle(Except.UnknownArgs, _argsList[0], true);
                }
                Exception? ex = null;

                // Parse the connection IPv4 address
                if (IPAddress.TryParse(_argsList[0], out IPAddress? addr))
                {
                    CmdArgs.Address = addr;
                }
                else  // Resolve the hostname
                {
                    CmdArgs.Address = Net.ResolveName(_argsList[0], out ex);
                }
                CmdArgs.HostName = _argsList[0];

                if (CmdArgs.Address.Equals(IPAddress.None))
                {
                    Error.Handle(Except.HostNotFound, _argsList[0], true, ex);
                }
                break;
            }
            default:  // Unexpected arguments
            {
                string argsStr = _argsList.Join(", ");

                if (_argsList[0].StartsWithValue('-'))
                {
                    Error.Handle(Except.UnknownArgs, argsStr, true);
                }
                Error.Handle(Except.InvalidArgs, argsStr, true);
                break;
            }
        }
    }

    /// <summary>
    ///  Determine whether the given index is a valid index of an
    ///  argument in the underlying command-line argument list.
    /// </summary>
    private bool ValidIndex(int index) => index >= 0 && index < _argsList.Count;

    /// <summary>
    ///  Get the value of the argument located at the given
    ///  index in the underlying command-line argument list.
    /// </summary>
    private string ArgsValueAt(int index)
    {
        if (!ValidIndex(index))
        {
            Error.Handle(Except.NamedArgs, _argsList[index - 1], true);
        }
        return _argsList[index];
    }

    /// <summary>
    ///  Parse and validate the network port number argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private void ParsePort(IndexedFlag idxFlag)
    {
        if (!ValidIndex(idxFlag.Index + 1))
        {
            Error.Handle(Except.NamedArgs, idxFlag.Flag, true);
        }
        string portStr = ArgsValueAt(idxFlag.Index + 1);

        if (!int.TryParse(portStr, out int port) || !Net.ValidPort(port))
        {
            Console.WriteLine(Usage);
            Error.Handle(Except.InvalidPort, portStr);
        }

        CmdArgs.Port = port;
        AddProcessedValueArg(idxFlag);
    }

    /// <summary>
    ///  Parse and validate the executable path argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private void ParseExecutable(IndexedFlag idxFlag)
    {
        if (!ValidIndex(idxFlag.Index + 1))
        {
            Error.Handle(Except.NamedArgs, idxFlag.Flag, true);
        }
        string exe = ArgsValueAt(idxFlag.Index + 1);

        if (!FileSys.ExistsOnPath(exe, out string? path))
        {
            Error.Handle(Except.ExePath, exe, true);
        }

        CmdArgs.ExePath = path;
        CmdArgs.PipeVariant = PipeType.Process;

        AddProcessedValueArg(idxFlag);
    }

    /// <summary>
    ///  Parse and validate the transfer file path argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private void ParseTransferPath(IndexedFlag idxFlag, TransferOpt transfer)
    {
        if (transfer is TransferOpt.None)
        {
            throw new ArgumentException("No file transfer option set.", nameof(transfer));
        }

        // No corresponding argument value
        if (!ValidIndex(idxFlag.Index + 1))
        {
            Error.Handle(Except.NamedArgs, idxFlag.Flag, true);
        }
        string? path = FileSys.ResolvePath(ArgsValueAt(idxFlag.Index + 1));

        // File path resolution failure
        if (path.IsNullOrEmpty())
        {
            Error.Handle(Except.FilePath, path, true);
        }
        string? parentPath = FileSys.ParentPath(path);

        // Parent path must exist for both collection and transmission
        if (parentPath.IsNullOrEmpty() || !FileSys.DirectoryExists(parentPath))
        {
            Error.Handle(Except.DirectoryPath, parentPath, true);
        }

        // File must exist to be transmitted
        if (!FileSys.FileExists(path) && transfer is TransferOpt.Transmit)
        {
            Error.Handle(Except.FilePath, path, true);
        }

        CmdArgs.FilePath = path;
        CmdArgs.PipeVariant = PipeType.File;
        CmdArgs.TransOpt = transfer;

        AddProcessedValueArg(idxFlag);
    }

    /// <summary>
    ///  Parse and validate the arbitrary payload argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private void ParseTextPayload(IndexedFlag idxFlag)
    {
        if (!ValidIndex(idxFlag.Index + 1))
        {
            Error.Handle(Except.NamedArgs, idxFlag.Flag, true);
        }
        string data = ArgsValueAt(idxFlag.Index + 1);

        if (data.IsNullOrEmpty())
        {
            Error.Handle(Except.Payload, idxFlag.Flag, true);
        }

        CmdArgs.Payload = data;
        CmdArgs.PipeVariant = PipeType.Text;

        AddProcessedValueArg(idxFlag);
    }

    /// <summary>
    ///  Mark the command-line argument at the given index as processed.
    /// </summary>
    private void AddProcessedArg(IndexedArg idxArg, int indexOffset = 0)
    {
        ThrowIf.Negative(idxArg.Index);
        ThrowIf.Negative(indexOffset);

        _processedIndexes.Add(idxArg.Index + indexOffset);
    }

    /// <summary>
    ///  Mark the command-line flag or flag alias value argument
    ///  immediately after the given index as processed.
    /// </summary>
    /// <remarks>
    ///  The command-line flag or flag alias will not be marked
    ///  as processed, only its corresponding value argument.
    /// </remarks>
    private void AddProcessedValueArg(IndexedFlag idxFlag) => AddProcessedArg(idxFlag, 1);
}
