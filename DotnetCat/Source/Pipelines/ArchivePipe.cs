using System;
using System.IO;
using System.IO.Compression;
using DotnetCat.Contracts;
using DotnetCat.Enums;
using ArgNullException = System.ArgumentNullException;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline class for directory/archive related data
    /// </summary>
    class ArchivePipe : FilePipe, IConnectable
    {
        private readonly string _zipPath;

        private bool _zipCreated;

        /// Initialize new object
        public ArchivePipe(string path) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                Error.Handle(Except.EmptyPath, "-s/--send");
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
                Error.Handle(Except.FilePath, path);
            }
            else
            {
                FileFound = true;
            }
            Source = new StreamReader(OpenFile(path, Error));
        }

        /// Activate pipline data flow between pipes
        public override void Connect()
        {
            _ = Source ?? throw new ArgNullException(nameof(Source));

            ToZipFile();
            base.Connect();
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
                throw new ArgumentNullException(nameof(FilePath));
            }

            if (!Directory.Exists(FilePath))
            {
                Dispose();
                Error.Handle(Except.FilePath, FilePath);
            }

            // Create the archive file
            try
            {
                ZipFile.CreateFromDirectory(FilePath, _zipPath);
                _zipCreated = true;
            }
            catch (Exception ex)
            {
                Dispose();
                Error.Handle(Except.Unhandled, ex.GetType().Name, ex);
            }
        }

        /// Unpack and cleanup archive files
        protected void Unzip()
        {
            /**
            * TODO: Change logic to call PathInfo, test/implement
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
            _ = FilePath ?? throw new ArgumentNullException(nameof(FilePath));

            if (!Directory.GetParent(FilePath).Exists)
            {
                string parent = Directory.GetParent(FilePath).FullName;
                Error.Handle(Except.DirectoryPath, parent);
            }

            ZipFile.ExtractToDirectory(_zipPath, FilePath);
            File.Delete(_zipPath);
        }

        /// Test if file path is valid
        private (bool exists, FileType type) PathInfo(string path)
        {
            FileType type = GetFileType(path);

            if (string.IsNullOrEmpty(path))
            {
                Error.Handle(Except.EmptyPath, path);
            }

            if (File.Exists(path) || Directory.Exists(path))
            {
                return (true, type);
            }
            return (false, FileType.None);
        }
    }
}
