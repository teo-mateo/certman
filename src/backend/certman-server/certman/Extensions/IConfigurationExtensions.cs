namespace certman.Extensions;

public static class IConfigurationExtensions
{
    public static string GetConnectionString(this IConfiguration config)
    {
        return $"Data Source={config["Database"]}";
    }
    
}