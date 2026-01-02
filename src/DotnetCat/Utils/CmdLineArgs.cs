using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using DotnetCat.Errors;
using DotnetCat.IO.Pipelines;
using static DotnetCat.Network.Constants;
using static DotnetCat.Utils.Constants;

namespace DotnetCat.Utils;

/// <summary>
///  DotnetCat command-line arguments.
/// </summary>
internal sealed class CmdLineArgs
{
    private static readonly ArgType[][] _invalidCombos;  // Invalid argument combinations

    private readonly List<ArgType> _parsedTypes;         // Parsed argument types

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static CmdLineArgs() => _invalidCombos =
    [
        [ArgType.Exec, ArgType.Output],
        [ArgType.Exec, ArgType.Send],
        [ArgType.Exec, ArgType.Text],
        [ArgType.Exec, ArgType.ZeroIo],
        [ArgType.Send, ArgType.Output],
        [ArgType.Send, ArgType.Text],
        [ArgType.Send, ArgType.ZeroIo],
        [ArgType.Text, ArgType.Output],
        [ArgType.Text, ArgType.ZeroIo],
        [ArgType.ZeroIo, ArgType.Listen],
        [ArgType.ZeroIo, ArgType.Output]
    ];

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public CmdLineArgs()
    {
        _parsedTypes = [];
        Help = Listen = Verbose = false;

        PipeVariant = PipeType.Stream;
        TransOpt = TransferOpt.None;

        Port = DEFAULT_PORT;
        Address = IPAddress.Any;
    }

    /// <summary>
    ///  Display extended usage information and exit.
    /// </summary>
    public bool Help { get; set; }

    /// <summary>
    ///  Run server and listen for inbound connection.
    /// </summary>
    public bool Listen { get; set; }

    /// <summary>
    ///  Using executable pipeline.
    /// </summary>
    public bool UsingExe => !ExePath.IsNullOrEmpty();

    /// <summary>
    ///  Enable verbose console output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    ///  Pipeline variant.
    /// </summary>
    public PipeType PipeVariant
    {
        get;
        set => field = ThrowIf.Undefined(value);
    }

    /// <summary>
    ///  File transfer option.
    /// </summary>
    public TransferOpt TransOpt
    {
        get;
        set => field = ThrowIf.Undefined(value);
    }

    /// <summary>
    ///  Connection port number.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///  Executable file path.
    /// </summary>
    public string? ExePath { get; set; }

    /// <summary>
    ///  Transfer file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    ///  Hostname of the connection IPv4 address.
    /// </summary>
    public string HostName
    {
        get => field ?? Address.ToString();
        set;
    }

    /// <summary>
    ///  User-defined string payload.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    ///  IPv4 address to use for connection.
    /// </summary>
    public IPAddress Address { get; set; }

    /// <summary>
    ///  Get the argument names of the given argument
    ///  type enumerators joined by a comma delimiter.
    /// </summary>
    public static string ArgNames([NotNull] IEnumerable<ArgType>? argTypes)
    {
        return ThrowIf.NullOrEmpty(argTypes).Select(ArgName).Join(", ");
    }

    /// <summary>
    ///  Add the given argument type to the underlying parsed argument types.
    /// </summary>
    public void AddParsedType(ArgType argType) => _parsedTypes.Add(argType);

    /// <summary>
    ///  Get all invalid argument combinations in the underlying parsed argument types.
    /// </summary>
    public IEnumerable<ArgType[]> InvalidCombinations()
    {
        return _invalidCombos.Where(_parsedTypes.Contains);
    }

    /// <summary>
    ///  Get the argument name of the given argument type enumerator.
    /// </summary>
    private static string ArgName(ArgType argType) => ThrowIf.Undefined(argType) switch
    {
        ArgType.Exec    => EXEC_FLAG,
        ArgType.Help    => HELP_FLAG,
        ArgType.Listen  => LISTEN_FLAG,
        ArgType.Output  => OUTPUT_FLAG,
        ArgType.Port    => PORT_FLAG,
        ArgType.Send    => SEND_FLAG,
        ArgType.Text    => TEXT_FLAG,
        ArgType.Verbose => VERBOSE_FLAG,
        ArgType.ZeroIo  => ZERO_IO_FLAG,
        _               => TARGET_ARG,
    };
}
