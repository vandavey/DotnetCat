namespace DotnetCat.Pipelines
{
    /// <summary>
    ///  Socket and file transfer option enumeration type
    /// </summary>
    internal enum TransferOpt : ushort
    {
        None,      // No [file]<->[socket] stream data redirection
        Collect,   // Redirect socket stream data to file stream
        Transmit   // Redirect file stream data to socket stream
    }
}
