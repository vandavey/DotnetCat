using System;
using System.IO;
using System.IO.Compression;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using DotnetCat.Handlers;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline class for directory/archive related data
    /// </summary>
    class ArchivePipe : FilePipe, IErrorHandled
    {
        private readonly string _zipPath;

        private bool _zipCreated;

        /// Initialize new object
        public ArchivePipe(string path) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                PipeError(Except.EmptyPath, "-s/--send");
            }

            /**
            * TODO: initialize Source/Dest
            **/ 
            _zipCreated = false;
            _zipPath = $"{path}.~dncat.zip";

            FilePath = path;
            PathType = GetFileType(path);

            if (PathType is FileType.None)
            {
                PipeError(Except.FilePath, path);
            }
            else
            {
                FileFound = true;
            }
            Source = new StreamReader(OpenFile(path));
        }

        /// Activate pipline data flow between pipes
        public override void Connect()
        {
            _ = Source ?? throw new ArgNullException(nameof(Source));

            ToZipFile();
            base.Connect();
        }

        /// Dispose of unmanaged resources and handle error
        public override void PipeError(Except type, string arg,
                                                    Exception ex = null,
                                                    Level level = Level.Error) {
            Dispose();
            ErrorHandler.Handle(type, arg, ex, level);
        }

        /// Release any unmanaged resources
        public override void Dispose()
        {
            if (_zipCreated)
            {
                File.Delete(_zipPath);
            }
            base.Dispose();
        }

        /// Create zip archive and add files
        protected void ToZipFile()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                throw new ArgNullException(nameof(FilePath));
            }

            // File path not found
            if (!Directory.Exists(FilePath))
            {
                PipeError(Except.FilePath, FilePath);
            }

            try // Create the archive file
            {
                ZipFile.CreateFromDirectory(FilePath, _zipPath);
                _zipCreated = true;
            }
            catch (Exception ex)
            {
                PipeError(Except.Unhandled, ex.GetType().Name, ex);
            }
        }

        /// Unpack and cleanup archive files
        protected void Unzip()
        {
            /**
            * TODO:: Change logic to call PathInfo, test/implement
            *     :: [ NOT READY FOR USE ]
            **/
            if (FileFound)
            {
                ZipFile.ExtractToDirectory(_zipPath, FilePath);
                File.Delete(_zipPath);
                return;
            }
            FileInfo info = new FileInfo(FilePath);
            //PathInfo()

            if (!File.Exists(_zipPath))
            {
                throw new FileNotFoundException(_zipPath);
            }
            _ = FilePath ?? throw new ArgNullException(nameof(FilePath));

            if (!Directory.GetParent(FilePath).Exists)
            {
                string parent = Directory.GetParent(FilePath).FullName;
                PipeError(Except.DirectoryPath, parent);
            }

            ZipFile.ExtractToDirectory(_zipPath, FilePath);
            File.Delete(_zipPath);
        }

        /// Determine file type of path if it exists
        private (bool exists, FileType type) PathInfo(string path)
        {
            FileType type = GetFileType(path);

            if (string.IsNullOrEmpty(path))
            {
                PipeError(Except.EmptyPath, path);
            }

            if (File.Exists(path) || Directory.Exists(path))
            {
                return (true, type);
            }
            return (false, FileType.None);
        }
    }
}
