using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DotnetCat.Errors;
using DotnetCat.Shell;
using DotnetCat.Utils;
using SpecialFolder = System.Environment.SpecialFolder;

namespace DotnetCat.IO;

/// <summary>
///  File system utility class.
/// </summary>
internal static class FileSys
{
    private static readonly string[] _envPaths;       // Local environment paths

    private static readonly string[] _exeExtensions;  // Executable file extensions

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static FileSys()
    {
        _envPaths = Command.GetEnvVariable("Path")?.Split(Path.PathSeparator) ?? [];
        _exeExtensions = [string.Empty, ".exe", ".ps1", ".sh", ".py", ".bat"];
    }

    /// <summary>
    ///  User home directory absolute file path.
    /// </summary>
    public static string UserProfile
    {
        get => Environment.GetFolderPath(SpecialFolder.UserProfile);
    }

    /// <summary>
    ///  Determine whether a file system entry exists at the given file path.
    /// </summary>
    public static bool Exists([NotNullWhen(true)] string? path)
    {
        return FileExists(path) || DirectoryExists(path);
    }

    /// <summary>
    ///  Determine whether a file entry exists at the given file path.
    /// </summary>
    public static bool FileExists([NotNullWhen(true)] string? path)
    {
        return File.Exists(ResolvePath(path));
    }

    /// <summary>
    ///  Determine whether a directory entry exists at the given file path.
    /// </summary>
    public static bool DirectoryExists([NotNullWhen(true)] string? path)
    {
        return Directory.Exists(ResolvePath(path));
    }

    /// <summary>
    ///  Get the absolute parent directory path from the given file path.
    /// </summary>
    [return: NotNullIfNotNull(nameof(path))]
    public static string? ParentPath(string? path)
    {
        string? parent = null;

        if (!path.IsNullOrEmpty())
        {
            parent = Directory.GetParent(path)?.FullName;
        }
        return ResolvePath(parent);
    }

    /// <summary>
    ///  Resolve the absolute file path of the given relative file path.
    /// </summary>
    [return: NotNullIfNotNull(nameof(path))]
    public static string? ResolvePath(string? path)
    {
        string? fullPath = null;

        // Ensure home and drive paths are properly interpreted
        if (!path.IsNullOrEmpty())
        {
            if (SysInfo.IsWindows() && path.EndsWithValue(Path.VolumeSeparatorChar))
            {
                path += Path.DirectorySeparatorChar;
            }
            fullPath = Path.GetFullPath(path.Replace("~", UserProfile));
        }
        return fullPath;
    }

    /// <summary>
    ///  Determine whether the given executable can
    ///  be located using the local environment path.
    /// </summary>
    public static bool ExistsOnPath([NotNull] string? exe,
                                    [NotNullWhen(true)] out string? path)
    {
        ThrowIf.NullOrEmpty(exe);
        path = FileExists(exe) ? exe : FindExecutable(exe);
        return path is not null;
    }

    /// <summary>
    ///  Search the local environment path for the given executable.
    /// </summary>
    private static string? FindExecutable([NotNull] string? exe)
    {
        ThrowIf.NullOrEmpty(exe);
        string exeName = Path.GetFileName(exe);

        IEnumerable<string> executables =
            from dir in _envPaths
            where DirectoryExists(dir)
            from file in Directory.GetFiles(dir)
            where file.EndsWith(exeName)
                || (!Path.HasExtension(exeName)
                    && _exeExtensions.Contains(Path.GetExtension(file))
                    && Path.GetFileNameWithoutExtension(file) == exeName)
            select file;

        return executables.FirstOrDefault();
    }
}
