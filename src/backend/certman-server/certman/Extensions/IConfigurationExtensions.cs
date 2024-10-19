namespace certman.Extensions;

public static class IConfigurationExtensions
{
    public static string GetConnectionString(this IConfiguration config) => $"Data Source={config["Database"]}";

    public static string GetWorkdir(this IConfiguration config) => config["Workdir"] ?? throw new Exception("Workdir not found in config");

    public static string GetStore(this IConfiguration config) => config["Store"] ?? throw new Exception("Store not found in config");
    
    public static string GetDatabase(this IConfiguration config) => config["Database"] ?? throw new Exception("Database not found in config");
}