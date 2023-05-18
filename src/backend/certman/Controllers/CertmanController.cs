using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace certman.Controllers;

public class CertmanController : ControllerBase
{
    protected readonly IConfiguration Config;

    // ctor with IConfiguration
    public CertmanController(IConfiguration config)
    {
        Config = config;
    }

    protected async Task<SqliteConnection> GetOpenConnection()
    {
        //get connection string
        var connectionString = Config.GetConnectionString("DefaultConnection");

        //open connection
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}