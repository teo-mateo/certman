using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace certman.Routes;

public static class DbUtils
{
    public static readonly Delegate CreateDb =
        async ([FromServices] IConfiguration config) =>
        {
            //get connection string
            var connectionString = config.GetConnectionString("DefaultConnection");

            //open connection
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // execute script from scripts/db.sql
            await using var scriptCommand = connection.CreateCommand();
            scriptCommand.CommandText = await File.ReadAllTextAsync("scripts/db.sql");
            await scriptCommand.ExecuteNonQueryAsync();

            await connection.CloseAsync();
            return "ok";
        };
}