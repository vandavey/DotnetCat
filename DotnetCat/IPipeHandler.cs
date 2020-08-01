using System;

namespace DotnetCat
{
    /// <summary>
    /// Interface for handling stream pipes
    /// </summary>
    interface IPipeHandler : ICloseable
    {
        /// Activate communication between pipe streams
        void ConnectPipes();

        /// Wait for pipes to be disconnected
        void WaitForExit();

        /// Determine if all pipes are connected
        bool AllPipesConnected();
    }
}
