using System.Data;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS.Commands.Storage;

public record RecreateDbTablesCommand : IRequest<Unit>;

public class RecreateDbTablesCommandHandler : CertmanHandler<RecreateDbTablesCommand, Unit>
{
    public RecreateDbTablesCommandHandler(IConfiguration config) : base(config) { }

    protected override async Task<Unit> ExecuteAsync(RecreateDbTablesCommand request, CancellationToken ctoken)
    {

        await RecreateDbTables(ctoken);
        return Unit.Value;
    }
    
    private void DeleteDbIfExists()
    {
        var connectionString = Config.GetConnectionString("DefaultConnection");
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var databaseFile = builder.DataSource;
        if (File.Exists(databaseFile))
        {
            File.Delete(databaseFile);
        }
    }

    private async Task RecreateDbTables(CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnectionAsync();
        // delete the db if it exists
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DROP TABLE IF EXISTS Certs; DROP TABLE IF EXISTS CACerts;";
        await deleteCommand.ExecuteNonQueryAsync(ctoken);
        
        // execute script from scripts/db.sql
        await using var scriptCommand = connection.CreateCommand();
        scriptCommand.CommandText = await System.IO.File.ReadAllTextAsync("Scripts/db.sql", ctoken);
        await scriptCommand.ExecuteNonQueryAsync(ctoken);
        
        SqliteConnection.ClearAllPools();
    }
}