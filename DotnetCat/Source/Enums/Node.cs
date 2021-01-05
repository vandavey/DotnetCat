namespace DotnetCat.Enums
{
    /// <summary>
    /// TCP socket node enumeration type
    /// </summary>
    enum Node : short
    {
        Client,  // TCP socket client (connect)
        Server   // TCP socket server (listen)
    }
}
