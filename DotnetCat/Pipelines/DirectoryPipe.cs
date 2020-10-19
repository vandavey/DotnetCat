using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using DotnetCat.Contracts;
using DotnetCat.Enums;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline class for directory and archive related data
    /// </summary>
    class DirectoryPipe : FileSysPipe, IConnectable
    {
        private string _dirPath, _zipPath;

        /// Initialize new DirectoryPipe
        public DirectoryPipe(string path) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            _dirPath = path;
            _zipPath = $"{path}.temp.zip";
        }

        /// Activate communication between the pipe streams
        public override void Connect()
        {
            if (Recursive && (IOAction == IOActionType.ReadFile))
            {
                CreateZip(_zipPath ??= $"{FilePath}.temp.zip");
            }
            // TODO: create temporary zip file
            base.Connect();
        }

        /// Release any unmanaged resources
        public override void Dispose()
        {
            if (Recursive && File.Exists(_zipPath))
            {
                File.Delete(_zipPath);
            }

            base.Dispose();
        }

        /// Create zip archive and add files
        protected void CreateZip(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath ??= _dirPath))
            {
                throw new ArgumentNullException(nameof(dirPath));
            }

            _zipPath ??= $"{dirPath}.temp.zip";

            if (!Directory.Exists(dirPath))
            {
                Error.Handle(ErrorType.DirectoryPath, dirPath);
            }

            _dirPath = dirPath;
            ZipFile.CreateFromDirectory(dirPath, _zipPath);
        }
    }
}
