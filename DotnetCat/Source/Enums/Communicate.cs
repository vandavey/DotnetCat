namespace DotnetCat.Enums
{
    /// <summary>
    /// Socket/file communication operation enumeration type
    /// </summary>
    enum Communicate : short
    {
        None,      // No file|<->|socket stream data redirection
        Collect,   // Redirect socket stream data to file stream
        Transmit,  // Redirect file stream data to socket stream
    }
}
