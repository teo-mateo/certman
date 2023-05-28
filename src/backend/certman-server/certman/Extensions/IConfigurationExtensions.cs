namespace certman.Extensions;

public static class IConfigurationExtensions
{
    public static string GetConnectionString(this IConfiguration config) => $"Data Source={config["Database"]}";

    public static string GetWorkdir(this IConfiguration config) => config["Workdir"];

    public static string GetStore(this IConfiguration config) => config["Store"];
    
    public static string GetDatabase(this IConfiguration config) => config["Database"];
}