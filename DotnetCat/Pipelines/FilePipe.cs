using System;
using System.IO;
using DotnetCat.Contracts;
using DotnetCat.Enums;

namespace DotnetCat.Pipelines
{
    /// <summary>
    /// Pipeline class for normal file related data
    /// </summary>
    class FilePipe : FileSysPipe, IConnectable
    {
        public FilePipe(StreamReader src, string path) : base()
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }
            else if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.Source = src;
            this.FilePath = path;
            this.Dest = new StreamWriter(CreateFile(path, Error));
            this.IOAction = IOActionType.WriteFile;
        }

        /// Initialize new FilePipe
        public FilePipe(string path, StreamWriter dest) : base()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            else if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            this.FilePath = path;
            this.Dest = dest;
            this.Source = new StreamReader(OpenFile(path, Error));
            this.IOAction = IOActionType.ReadFile;
        }
    }
}
