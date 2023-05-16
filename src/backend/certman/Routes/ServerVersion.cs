namespace certman.Routes;

public static class ServerVersion
{
    public static readonly Delegate GetServerVersion =
        () => new
        {
            ServerVersion = "1.0"
        };
}