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
    private static readonly string[] _envPaths;        // Environment path directories
    private static readonly string[] _pathExtensions;  // Environment path file extensions

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static FileSys()
    {
        _envPaths = EnvironmentPaths();
        _pathExtensions = PathExtensions();
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
        path = null;

        if (FileExists(exe))
        {
            path = ResolvePath(exe);
        }
        else if (!Path.IsPathRooted(exe))
        {
            path = FindExecutable(exe);
        }
        return path is not null;
    }

    /// <summary>
    ///  Get the directory paths defined by the local environment path variable.
    /// </summary>
    private static string[] EnvironmentPaths()
    {
        if (!Command.TryEnvVariable("PATH", out string? envPath))
        {
            throw new InvalidOperationException("Failed to get local environment paths.");
        }
        return envPath.Split(Path.PathSeparator);
    }

    /// <summary>
    ///  Get the locally supported environment path executable file extensions.
    /// </summary>
    private static string[] PathExtensions()
    {
        string[] extensions;

        if (SysInfo.IsWindows())
        {
            if (Command.TryEnvVariable("PATHEXT", out string? pathExt))
            {
                extensions = [.. pathExt.Split(Path.PathSeparator)];
            }
            else  // Use common Windows extensions
            {
                extensions = [".exe", ".bat", ".cmd", ".ps1", ".py"];
            }
        }
        else  // Use Linux extensions
        {
            extensions = [".sh", ".py", ".bin", ".run", ".elf"];
        }

        return [.. extensions.Select(e => e.ToLower())];
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
            where file.EndsWith(Path.DirectorySeparatorChar + exeName)
                || (!Path.HasExtension(exeName)
                    && Path.GetFileNameWithoutExtension(file) == exeName
                    && _pathExtensions.Contains(Path.GetExtension(file)))
            select file;

        return executables.FirstOrDefault();
    }
}
