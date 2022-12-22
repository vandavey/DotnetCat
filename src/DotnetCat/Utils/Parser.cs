using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Network;

namespace DotnetCat.Utils;

/// <summary>
///  Command-line argument parser and validator.
/// </summary>
internal class Parser
{
    private static readonly string _title;  // Application title

    private readonly string _eol;           // Platform EOL string
    private readonly string _help;          // Help information

    private readonly CmdLineArgs _args;     // Command-line arguments

    private List<string> _argsList;         // Command-line argument list

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Parser() => _title = Program.OS is Platform.Nix ? "dncat" : "dncat.exe";

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public Parser()
    {
        _eol = Environment.NewLine;
        _help = GetHelpMessage();

        _args = new CmdLineArgs();
        _argsList = new List<string>();
    }

    /// Application repository URL
    public static string Repo => "https://github.com/vandavey/DotnetCat";

    /// Application usage string
    public static string Usage => $"Usage: {_title} [OPTIONS] TARGET";

    /// <summary>
    ///  Determine whether any of the help flags (-h, -?, --help) exist
    ///  in the given command-line argument array.
    /// </summary>
    public static bool NeedsHelp(string[] args)
    {
        int count = (from string arg in args.ToList()
                     where arg == "--help"
                         || (arg.Length > 1
                             && arg[0] == '-'
                             && arg[1] != '-'
                             && (arg.Contains('h') || arg.Contains('?')))
                     select arg).Count();

        return count > 0;
    }

    /// <summary>
    ///  Get the index of the given flag (--foo) or flag alias (-f) argument
    ///  in the specified command-line argument list.
    /// </summary>
    public static int IndexOfFlag(List<string>? args,
                                  string flag,
                                  char? alias = default) {
        if (flag.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(flag));
        }
        List<string> argsList = args is null ? new() : args.ToList();

        if (flag == "-")
        {
            return argsList.IndexOf(flag);
        }
        alias ??= flag.FirstOrDefault(char.IsLetter);

        IEnumerable<int> flagIndexes = from string arg in argsList
                                       where arg == flag
                                           || (arg.Contains(alias ?? '\0')
                                               && arg[0] == '-'
                                               && arg[1] != '-')
                                       select argsList.IndexOf(flag);

        return flagIndexes.Any() ? flagIndexes.First() : -1;
    }

    /// <summary>
    ///  Parse and validate all the arguments in the given
    ///  command-line argument array.
    /// </summary>
    public CmdLineArgs Parse(string[] args)
    {
        _argsList = DefragArguments(args.ToList());

        ParseCharArgs();
        ParseFlagArgs();
        ParsePositionalArgs();

        return _args;
    }

    /// <summary>
    ///  Get index of the given flag alias (-f) argument in the underlying
    ///  command-line argument list.
    /// </summary>
    public int IndexOfAlias(char alias)
    {
        IEnumerable<int> aliasIndexes = from string arg in _argsList.ToList()
                                        where arg.Contains(alias)
                                            && arg[0] == '-'
                                            && arg[1] != '-'
                                        select _argsList.IndexOf(arg);

        return aliasIndexes.Any() ? aliasIndexes.First() : -1;
    }

    /// <summary>
    ///  Get the index of the given flag (--foo) or flag alias (-f) argument
    ///  in the underlying command-line argument list.
    /// </summary>
    public int IndexOfFlag(string flag, char? alias = default)
    {
        return IndexOfFlag(_argsList, flag, alias);
    }

    /// <summary>
    ///  Write the extended application usage information to the standard
    ///  console output stream and exit the application.
    /// </summary>
    public void PrintHelp()
    {
        Console.WriteLine(_help);
        Environment.Exit(0);
    }

    /// <summary>
    ///  Defragment the given fragmented command-line arguments so
    ///  quoted strings are interpreted as single arguments.
    /// </summary>
    private static List<string> DefragArguments(List<string> args)
    {
        int delta = 0;
        List<string> list = args.ToList();

        IEnumerable<(int, string, char, bool)> results;

        results = from string arg in args
                  let quote = arg.FirstOrDefault()
                  let valid = arg.EndsWith(quote) && arg.Length >= 2
                  where arg.StartsWith("'") || arg.StartsWith("\"")
                  select (args.IndexOf(arg), arg, quote, valid);

        foreach ((int bolPos, string bolArg, char quote, bool valid) in results)
        {
            // Skip processed arguments
            if (delta > 0)
            {
                delta -= 1;
                continue;
            }
            int listIndex = list.IndexOf(bolArg);

            // Non-fragmented string
            if (valid)
            {
                list[listIndex] = bolArg[1..^1];
                continue;
            }

            (int eolPos, string eolArg) = (from string arg in args
                                           let pos = args.IndexOf(arg, bolPos + 1)
                                           where pos > bolPos
                                               && (arg == quote.ToString()
                                                   || arg.EndsWith(quote))
                                           select (pos, arg)).FirstOrDefault();
            if (eolArg is null)
            {
                string arg = args.ToArray()[bolPos..].Join(", ");
                Error.Handle(Except.StringEol, arg, true);
            }
            delta = eolPos - bolPos;

            // Append fragments to the list argument
            for (int i = bolPos + 1; i < bolPos + delta + 1; i++)
            {
                list[listIndex] += $" {args[i]}";
                list.Remove(args[i]);
            }

            string defragged = list[listIndex];
            list[listIndex] = defragged[1..^1];
        }
        return list;
    }

    /// <summary>
    ///  Parse the named flag alias arguments in the underlying command-line
    ///  argument list. Flag alias arguments begin with one dash (-f).
    /// </summary>
    private void ParseCharArgs()
    {
        IEnumerable<(int, string)> results = from arg in _argsList.ToList()
                                             let index = IndexOfFlag(arg)
                                             where arg.Length >= 2
                                                 && arg[0] == '-'
                                                 && arg[1] != '-'
                                             select (index, arg);

        foreach ((int index, string arg) in results)
        {
            if (arg.Contains('l'))  // Listen for connection
            {
                _args.Listen = true;
                RemoveAlias(index, 'l');
            }

            if (arg.Contains('v'))  // Verbose output
            {
                _args.Verbose = true;
                RemoveAlias(index, 'v');
            }

            if (arg.Contains('z'))  // Zero-IO (test connection)
            {
                _args.PipeVariant = PipeType.Status;
                RemoveAlias(index, 'z');
            }

            if (arg.Contains('d'))  // Debug output
            {
                _args.Debug = _args.Verbose = Error.Debug = true;
                RemoveAlias(index, 'd');
            }

            if (arg.Contains('p'))  // Connection port
            {
                _args.Port = GetPort(index);
                RemoveAlias(index, 'p', removeValue: true);
            }

            if (arg.Contains('e'))  // Executable path
            {
                _args.ExePath = GetExecutable(index);
                RemoveAlias(index, 'e', removeValue: true);
            }

            if (arg.Contains('o'))  // Receive file data
            {
                _args.FilePath = GetTransfer(index);
                _args.TransOpt = TransferOpt.Collect;
                RemoveAlias(index, 'o', removeValue: true);
            }

            if (arg.Contains('s'))  // Send file data
            {
                _args.FilePath = GetTransfer(index);
                _args.TransOpt = TransferOpt.Transmit;
                RemoveAlias(index, 's', removeValue: true);
            }

            if (arg.Contains('t'))  // Send string data
            {
                _args.Payload = GetTextPayload(index);
                RemoveAlias(index, 't', removeValue: true);
            }

            if (ArgsValueAt(index) == "-")
            {
                _argsList.RemoveAt(IndexOfFlag("-"));
            }
        }
    }

    /// <summary>
    ///  Parse the named flag arguments in the underlying command-line
    ///  argument list. Flag arguments begin with two dashes (--foo).
    /// </summary>
    private void ParseFlagArgs()
    {
        IEnumerable<(int, string)> results = from string arg in _argsList.ToList()
                                             let index = IndexOfFlag(arg)
                                             where arg.StartsWith("--")
                                             select (index, arg);

        foreach ((int index, string arg) in results)
        {
            switch (arg)
            {
                case "--listen":   // Listen for connection
                {
                    _args.Listen = true;
                    _argsList.RemoveAt(index);
                    break;
                }
                case "--verbose":  // Verbose output
                {
                    _args.Verbose = true;
                    _argsList.RemoveAt(index);
                    break;
                }
                case "--debug":    // Debug output
                {
                    _args.Debug = _args.Verbose = Error.Debug = true;
                    _argsList.RemoveAt(index);
                    break;
                }
                case "--zero-io":  // Zero-IO (test connection)
                {
                    _args.PipeVariant = PipeType.Status;
                    _argsList.RemoveAt(index);
                    break;
                }
                case "--port":     // Connection port
                {
                    _args.Port = GetPort(index);
                    RemoveFlag(ArgsValueAt(index));
                    break;
                }
                case "--exec":     // Executable path
                {
                    _args.ExePath = GetExecutable(index);
                    RemoveFlag(ArgsValueAt(index));
                    break;
                }
                case "--text":     // Send string data
                {
                    _args.Payload = GetTextPayload(index);
                    RemoveFlag(ArgsValueAt(index));
                    break;
                }
                case "--output":   // Receive file data
                {
                    _args.FilePath = GetTransfer(index);
                    _args.TransOpt = TransferOpt.Collect;
                    RemoveFlag(ArgsValueAt(index));
                    break;
                }
                case "--send":     // Send file data
                {
                    _args.FilePath = GetTransfer(index);
                    _args.TransOpt = TransferOpt.Transmit;
                    RemoveFlag(ArgsValueAt(index));
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    ///  Parse the positional (required) arguments in the underlying
    ///  command-line argument list.
    /// </summary>
    private void ParsePositionalArgs()
    {
        switch (_argsList.Count)
        {
            case 0:   // Missing TARGET
            {
                if (!_args.Listen)
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
                Exception? ex = default;

                // Parse the connection IPv4 address
                if (IPAddress.TryParse(_argsList[0], out IPAddress? addr))
                {
                    _args.Address = addr;
                }
                else  // Resolve the hostname
                {
                    (_args.Address, ex) = Net.ResolveName(_argsList[0]);
                }
                _args.HostName = _argsList[0];

                if (_args.Address == IPAddress.Any)
                {
                    Error.Handle(Except.HostNotFound, _argsList[0], true, ex: ex);
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
    ///  Get the extended application usage information message.
    /// </summary>
    private string GetHelpMessage()
    {
        string helpMessage = $"""
            DotnetCat ({Repo})
            {Usage}{_eol}
            Remote command shell application{_eol}
            Positional Arguments:
              TARGET                    Remote or local IPv4 address{_eol}
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
              -t DATA, --text DATA      Send string to remote host{_eol}
            Usage Examples:
              {_title} --listen --exec powershell.exe
              {_title} -d -p 44444 localhost
              {_title} -vo test.txt -p 2009 192.168.1.9{_eol}
            """;
        return helpMessage;
    }

    /// <summary>
    ///  Remove the given flag alias (-f) character from the argument located at
    ///  the specified index in the underlying command-line argument list.
    /// </summary>
    private void RemoveAlias(int index, char alias, bool removeValue = false)
    {
        if (!ValidIndex(index) || (removeValue && !ValidIndex(index + 1)))
        {
            throw new IndexOutOfRangeException(nameof(index));
        }
        _argsList[index] = _argsList[index].Replace(alias.ToString(), "");

        if (removeValue)
        {
            _argsList.RemoveAt(index + 1);
        }
    }

    /// <summary>
    ///  Determine whether the given index is a valid index of an argument
    ///  in the underlying command-line argument list.
    /// </summary>
    private bool ValidIndex(int index) => index >= 0 && index < _argsList.Count;

    /// <summary>
    ///  Remove the given flag (--foo) argument from the underlying command-line
    ///  argument list. Optionally remove the corresponding value argument.
    /// </summary>
    private void RemoveFlag(string arg, bool removeValue = true)
    {
        int index = IndexOfFlag(arg);

        for (int i = 0; i < (removeValue ? 2 : 1); i++)
        {
            _argsList.RemoveAt(index);
        }
    }

    /// <summary>
    ///  Get the value of the argument located at the given index in
    ///  the underlying command-line argument list.
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
    ///  Parse and validate the network port number argument located at the
    ///  given index in the underlying command-line argument list.
    /// </summary>
    private int GetPort(int index)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, _argsList[index], true);
        }
        string portStr = ArgsValueAt(index + 1);

        if (!int.TryParse(portStr, out int port) || !Net.IsValidPort(port))
        {
            Console.WriteLine(Usage);
            Error.Handle(Except.InvalidPort, portStr);
        }
        return port;
    }

    /// <summary>
    ///  Parse and validate the executable path argument located at the
    ///  given index in the underlying command-line argument list.
    /// </summary>
    private string? GetExecutable(int index)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, _argsList[index], true);
        }

        string exec = ArgsValueAt(index + 1);
        (string? path, bool exists) = FileSys.ExistsOnPath(exec);

        if (!exists)
        {
            Error.Handle(Except.ExePath, exec, true);
        }
        _args.PipeVariant = PipeType.Process;

        return path;
    }

    /// <summary>
    ///  Parse and validate the transfer file path argument located at the
    ///  given index in the underlying command-line argument list.
    /// </summary>
    private string GetTransfer(int index)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, _argsList[index], true);
        }
        int pathPos = index + 1;

        string path = FileSys.ResolvePath(ArgsValueAt(pathPos)) ?? string.Empty;
        string parentPath = Directory.GetParent(path)?.FullName ?? string.Empty;

        bool pathExists = FileSys.FileExists(path);
        bool parentExists = FileSys.DirectoryExists(parentPath);

        if (!pathExists && _args.TransOpt is TransferOpt.Transmit)
        {
            Error.Handle(Except.FilePath, path, true);
        }
        else if (!parentExists && _args.TransOpt is TransferOpt.Collect)
        {
            Error.Handle(Except.FilePath, parentPath, true);
        }
        _args.PipeVariant = PipeType.File;

        return path;
    }

    /// <summary>
    ///  Parse and validate the arbitrary payload argument located at the
    ///  given index in the underlying command-line argument list.
    /// </summary>
    private string GetTextPayload(int index)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, _argsList[index], true);
        }
        string data = ArgsValueAt(index + 1);

        if (data.IsNullOrEmpty())
        {
            Error.Handle(Except.Payload, _argsList[index], true);
        }
        _args.PipeVariant = PipeType.Text;

        return data;
    }
}
