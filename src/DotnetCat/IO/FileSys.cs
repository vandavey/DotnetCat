using System;
using System.IO;
using System.Linq;
using DotnetCat.Shell;
using DotnetCat.Utils;
using SpecialFolder = System.Environment.SpecialFolder;

namespace DotnetCat.IO;

/// <summary>
///  File system utility class.
/// </summary>
internal static class FileSys
{
    private static readonly string _userHomePath;     // User home path

    private static readonly string[] _envPaths;       // Environment path

    private static readonly string[] _exeExtensions;  // Executable files

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static FileSys()
    {
        _exeExtensions = new string[]
        {
            "",     // Linux binary (ELF)
            "exe",  // Windows binary (PE)
            "bat",  // Batch script
            "ps1",  // PowerShell script
            "py",   // Python script
            "sh"    // Bash/shell script
        };
        string envVar = Command.GetEnvVariable("PATH") ?? string.Empty;

        _envPaths = envVar.Split(Path.PathSeparator);
        _userHomePath = GetUserHomePath();
    }

    /// <summary>
    ///  Get the absolute file path of the current user home directory.
    /// </summary>
    public static string GetUserHomePath()
    {
        return Environment.GetFolderPath(SpecialFolder.UserProfile);
    }

    /// <summary>
    ///  Determine whether a file system entry exists at the given file path.
    /// </summary>
    public static bool Exists(string? path)
    {
        return FileExists(path) || DirectoryExists(path);
    }

    /// <summary>
    ///  Determine whether a file entry exists at the given file path.
    /// </summary>
    public static bool FileExists(string? path)
    {
        bool exists = false;

        if (!path.IsNullOrEmpty())
        {
            exists = File.Exists(ResolvePath(path));
        }
        return exists;
    }

    /// <summary>
    ///  Determine whether a directory entry exists at the given file path.
    /// </summary>
    public static bool DirectoryExists(string? path)
    {
        bool exists = false;

        if (!path.IsNullOrEmpty())
        {
            exists = Directory.Exists(ResolvePath(path));
        }
        return exists;
    }

    /// <summary>
    ///  Get the file name from the given file path and optionally
    ///  exclude the file extension.
    /// </summary>
    public static string? GetFileName(string? path, bool withExt = true)
    {
        string? fileName = null;

        if (!path.IsNullOrEmpty())
        {
            // Include file extension
            if (withExt)
            {
                fileName = Path.GetFileName(path);
            }
            else  // Exclude extension
            {
                fileName = Path.GetFileNameWithoutExtension(path);
            }
        }
        return fileName;
    }

    /// <summary>
    ///  Resolve the absolute file path of the given relative file path.
    /// </summary>
    public static string? ResolvePath(string? path)
    {
        string fullPath = path ?? string.Empty;

        if (!fullPath.IsNullOrEmpty())
        {
            // Ensure drives are properly interpreted
            if (fullPath.EndsWithValue(":"))
            {
                fullPath += Path.DirectorySeparatorChar;
            }

            // Ensure home path is properly interpreted
            fullPath = Path.GetFullPath(fullPath.Replace("~", _userHomePath));
        }
        return fullPath;
    }

    /// <summary>
    ///  Determine whether the given executable file name can be found
    ///  by searching the local environment path.
    /// </summary>
    public static (string? path, bool exists) ExistsOnPath(string? exe)
    {
        if (exe.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(exe));
        }
        (string? path, bool exists) = (exe, Exists(exe));

        if (!exists)
        {
            (path, exists) = (path = FindExecutable(path), Exists(path));
        }
        return (path, exists);
    }

    /// <summary>
    ///  Search the local environment path for the given executable name.
    /// </summary>
    private static string? FindExecutable(string? exeName)
    {
        if (exeName.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(exeName));
        }
        string? fullPath = default;

        if (!File.Exists(exeName))
        {
            foreach (string path in _envPaths)
            {
                if (!DirectoryExists(path))
                {
                    continue;
                }

                if ((fullPath = GetFullExePath(path, exeName)) is not null)
                {
                    break;
                }
            }
        }
        return fullPath;
    }

    /// <summary>
    ///  Search files in the given directory for a file whose name
    ///  matches the specified executable name.
    /// </summary>
    private static string? GetFullExePath(string dirPath, string? exeName)
    {
        string? fullPath = default;

        string? fileName = (from string file in Directory.GetFiles(dirPath)
                            let name = GetFileName(file, false)
                            where name == GetFileName(exeName, false)
                            select name).FirstOrDefault();

        // Try to match executable extension
        if (fileName is not null)
        {
            foreach (string ext in _exeExtensions)
            {
                string newExeName = fileName;

                if (ext != string.Empty)
                {
                    newExeName = Path.ChangeExtension(newExeName, ext);
                }
                string testPath = Path.Combine(dirPath, newExeName);

                if (File.Exists(testPath))
                {
                    fullPath = testPath;
                    break;
                }
            }
        }
        return fullPath;
    }
}