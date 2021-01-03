using System;
using System.IO;
using System.IO.Compression;
using DotnetCat.Contracts;
using DotnetCat.Enums;

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

            // TODO: initialize this.Source/this.Dest
            _zipCreated = false;
            _zipPath = $"{path}.~dncat.zip";

            this.FilePath = path;
            this.PathType = GetFileType(path);

            if (PathType == FileType.None)
            {
                Error.Handle(Except.FilePath, path);
            }
            else
            {
                this.FileFound = true;
            }
            this.Source = new StreamReader(OpenFile(path, Error));
        }

        /// Activate pipline data flow between pipes
        public override void Connect()
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }

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
                throw ex;
            }
        }

        /// Unpack and cleanup archive files
        protected void Unzip()
        {
            if (FileFound)
            {
                ZipFile.ExtractToDirectory(_zipPath, FilePath);
                File.Delete(_zipPath);
                return;
            }
            FileInfo info = new FileInfo(FilePath);
            //PathInfo()

            // TODO: change logic to call PathInfo?
            if (!File.Exists(_zipPath))
            {
                throw new FileNotFoundException(_zipPath);
            }

            if (FilePath == null)
            {
                throw new ArgumentNullException(nameof(FilePath));
            }
            else if (!Directory.GetParent(FilePath).Exists)
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
