using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DotnetCat.Errors;
using DotnetCat.IO.FileSystem;
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
        _help = GetHelp();

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
        int count = (from arg in args.ToList()
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

        if (flag == "-")
        {
            return args?.IndexOf(flag) ?? -1;
        }

        alias ??= flag.Where(c => char.IsLetter(c)).FirstOrDefault();

        List<int> query = (from arg in args
                           where arg == flag
                               || (arg.Contains(alias ?? '\0')
                                   && arg[0] == '-'
                                   && arg[1] != '-')
                           select args?.IndexOf(flag) ?? -1).ToList();

        return query.Count > 0 ? query[0] : -1;
    }

    /// <summary>
    ///  Parse and validate all the arguments in the given
    ///  command-line argument array.
    /// </summary>
    public CmdLineArgs Parse(string[] args)
    {
        _argsList = DefragArguments(args);

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
        List<int> query = (from arg in _argsList.ToList()
                           where arg.Contains(alias)
                               && arg[0] == '-'
                               && arg[1] != '-'
                           select _argsList.IndexOf(arg)).ToList();

        return query.Count > 0 ? query[0] : -1;
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
    private static List<string> DefragArguments(string[] args)
    {
        int delta = 0;
        List<string> list = args.ToList();

        var bolQuery = from arg in args
                       let pos = Array.IndexOf(args, arg)
                       let quote = arg.FirstOrDefault()
                       let valid = arg.EndsWith(quote) && arg.Length >= 2
                       where arg.StartsWith("'") || arg.StartsWith("\"")
                       select new { arg, pos, quote, valid };

        foreach (var item in bolQuery)
        {
            // Skip processed arguments
            if (delta > 0)
            {
                delta -= 1;
                continue;
            }
            int listIndex = list.IndexOf(item.arg);

            // Non-fragmented string
            if (item.valid)
            {
                list[listIndex] = item.arg[1..(item.arg.Length - 1)];
                continue;
            }

            var eolQuery = (from arg in args
                            let pos = Array.IndexOf(args, arg, item.pos + 1)
                            where pos > item.pos
                                && (arg == item.quote.ToString()
                                    || arg.EndsWith(item.quote))
                            select new { arg, pos }).FirstOrDefault();

            // Missing EOL (quote)
            if (eolQuery is null)
            {
                string arg = string.Join(", ", args[item.pos..]);
                Error.Handle(Except.StringEOL, arg, true);
            }
            else  // Calculate position delta
            {
                delta = eolQuery.pos - item.pos;
            }

            int endIndex = item.pos + delta;

            // Append fragments and remove duplicates
            for (int i = item.pos + 1; i < endIndex + 1; i++)
            {
                list[listIndex] += $" {args[i]}";
                list.Remove(args[i]);
            }

            string defragged = list[listIndex];
            list[listIndex] = defragged[1..(defragged.Length - 1)];
        }
        return list;
    }

    /// <summary>
    ///  Parse the named flag alias arguments in the underlying command-line
    ///  argument list. Flag alias arguments begin with one dash (-f).
    /// </summary>
    private void ParseCharArgs()
    {
        var query = from arg in _argsList.ToList()
                    let index = IndexOfFlag(arg)
                    where (arg.Length >= 2) && (arg[0] == '-') && (arg[1] != '-')
                    select new { arg, index };

        foreach (var item in query)
        {
            if (item.arg.Contains('l'))  // Listen for connection
            {
                _args.Listen = true;
                RemoveAlias(item.index, 'l');
            }

            if (item.arg.Contains('v'))  // Verbose output
            {
                _args.Verbose = true;
                RemoveAlias(item.index, 'v');
            }

            if (item.arg.Contains('z'))  // Zero-IO (test connection)
            {
                _args.PipeVariant = PipeType.Status;
                RemoveAlias(item.index, 'z');
            }

            if (item.arg.Contains('d'))  // Debug output
            {
                _args.Debug = _args.Verbose = Error.Debug = true;
                RemoveAlias(item.index, 'd');
            }

            if (item.arg.Contains('p'))  // Connection port
            {
                _args.Port = GetPort(item.index);
                RemoveAlias(item.index, 'p', removeValue: true);
            }

            if (item.arg.Contains('e'))  // Executable path
            {
                _args.ExePath = GetExecutable(item.index);
                RemoveAlias(item.index, 'e', removeValue: true);
            }

            if (item.arg.Contains('o'))  // Receive file data
            {
                _args.FilePath = GetTransfer(item.index);
                _args.TransOpt = TransferOpt.Collect;
                RemoveAlias(item.index, 'o', removeValue: true);
            }

            if (item.arg.Contains('s'))  // Send file data
            {
                _args.FilePath = GetTransfer(item.index);
                _args.TransOpt = TransferOpt.Transmit;
                RemoveAlias(item.index, 's', removeValue: true);
            }

            if (item.arg.Contains('t'))  // Send string data
            {
                _args.Payload = GetTextPayload(item.index);
                RemoveAlias(item.index, 't', removeValue: true);
            }

            if (ArgsValueAt(item.index) == "-")
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
        var query = from arg in _argsList.ToList()
                    let index = IndexOfFlag(arg)
                    where arg.StartsWith("--")
                    select new { arg, index };

        foreach (var item in query)
        {
            switch (item.arg)
            {
                case "--listen":   // Listen for connection
                {
                    _args.Listen = true;
                    _argsList.RemoveAt(item.index);
                    break;
                }
                case "--verbose":  // Verbose output
                {
                    _args.Verbose = true;
                    _argsList.RemoveAt(item.index);
                    break;
                }
                case "--debug":    // Debug output
                {
                    _args.Debug = _args.Verbose = Error.Debug = true;
                    _argsList.RemoveAt(item.index);
                    break;
                }
                case "--zero-io":  // Zero-IO (test connection)
                {
                    _args.PipeVariant = PipeType.Status;
                    _argsList.RemoveAt(item.index);
                    break;
                }
                case "--port":     // Connection port
                {
                    _args.Port = GetPort(item.index);
                    RemoveFlag(ArgsValueAt(item.index));
                    break;
                }
                case "--exec":     // Executable path
                {
                    _args.ExePath = GetExecutable(item.index);
                    RemoveFlag(ArgsValueAt(item.index));
                    break;
                }
                case "--text":     // Send string data
                {
                    _args.Payload = GetTextPayload(item.index);
                    RemoveFlag(ArgsValueAt(item.index));
                    break;
                }
                case "--output":   // Receive file data
                {
                    _args.FilePath = GetTransfer(item.index);
                    _args.TransOpt = TransferOpt.Collect;
                    RemoveFlag(ArgsValueAt(item.index));
                    break;
                }
                case "--send":     // Send file data
                {
                    _args.FilePath = GetTransfer(item.index);
                    _args.TransOpt = TransferOpt.Transmit;
                    RemoveFlag(ArgsValueAt(item.index));
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
                    Error.Handle(Except.InvalidAddr, _argsList[0], true, ex: ex);
                }
                break;
            }
            default:  // Unexpected arguments
            {
                string argsStr = _argsList.ToArray().Join(", ");

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
    private string GetHelp() => string.Join(_eol, new string[]
    {
        $"DotnetCat ({Repo})",
        $"{Usage}{_eol}",
        $"Remote command shell application{_eol}",
        "Positional Arguments:",
        $"  TARGET                    Remote or local IPv4 address{_eol}",
        "Optional Arguments:",
        "  -h/-?,   --help           Show this help message and exit",
        "  -v,      --verbose        Enable verbose console output",
        "  -d,      --debug          Output verbose error information",
        "  -l,      --listen         Listen for incoming connections",
        "  -z,      --zero-io        Report connection status only",
        "  -p PORT, --port PORT      Specify port to use for endpoint.",
        "                            (Default: 44444)",
        "  -e EXEC, --exec EXEC      Executable process file path",
        "  -o PATH, --output PATH    Receive file from remote host",
        "  -s PATH, --send PATH      Send local file or folder",
        $"  -t DATA, --text DATA      Send string to remote host{_eol}",
        "Usage Examples:",
        $"  {_title} --listen --exec powershell.exe",
        $"  {_title} -d -p 44444 localhost",
        $"  {_title} -vo test.txt -p 2009 192.168.1.9 {_eol}",
    });

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
