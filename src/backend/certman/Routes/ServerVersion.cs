namespace certman.Routes;

public static class ServerVersion
{
    public static readonly Delegate Get =
        () => new
        {
            ServerVersion = "1.0"
        };
}