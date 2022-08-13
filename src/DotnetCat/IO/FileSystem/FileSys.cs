using System;
using System.IO;
using System.Linq;
using DotnetCat.Shell.Commands;
using DotnetCat.Utils;
using SpecialFolder = System.Environment.SpecialFolder;

namespace DotnetCat.IO.FileSystem
{
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
            bool exists = !path.IsNullOrEmpty();

            if (exists)
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
            bool exists = !path.IsNullOrEmpty();

            if (exists)
            {
                exists = Directory.Exists(ResolvePath(path));
            }
            return exists;
        }

        /// <summary>
        ///  Get the file from the given file path and optionally
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

                // Ensure home profile is properly interpreted
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

            // Resolve absolute path
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
            bool exePathFound = false;

            // Search environment path directories
            if (!File.Exists(exeName))
            {
                foreach (string path in _envPaths)
                {
                    if (!DirectoryExists(path))
                    {
                        continue;
                    }

                    string? fileName = (from file in Directory.GetFiles(path)
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
                            string newPath = Path.Combine(path, newExeName);

                            // Executable match found
                            if (File.Exists(newPath))
                            {
                                fullPath = newPath;
                                exePathFound = true;
                                break;
                            }
                        }
                    }

                    // Terminate file search
                    if (exePathFound)
                    {
                        break;
                    }
                }
            }
            return fullPath;
        }
    }
}
