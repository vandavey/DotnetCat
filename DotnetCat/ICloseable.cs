using System;

namespace DotnetCat
{
    /// <summary>
    /// Interface for releasing unmanaged resources
    /// </summary>
    interface ICloseable
    {
        /// Release any unmanaged resources
        void Close();
    }
}
