using certman.Extensions;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS.Commands.Storage;

public record RecreateDbTablesCommand : IRequest<Unit>;

public class RecreateDbTablesCommandHandler(IConfiguration config, ILogger<RecreateDbTablesCommandHandler> logger)
    : CertmanHandler<RecreateDbTablesCommand, Unit>(config, logger)
{
    protected override async Task<Unit> ExecuteAsync(RecreateDbTablesCommand request, CancellationToken ctoken)
    {

        await RecreateDbTables(ctoken);
        return Unit.Value;
    }
    
    private void DeleteDbIfExists()
    {
        var connectionString = _config.GetConnectionString();
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

        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DROP TABLE IF EXISTS Certs; DROP TABLE IF EXISTS CACerts;";
        await deleteCommand.ExecuteNonQueryAsync(ctoken);
        
        // execute script from scripts/db.sql
        await using var scriptCommand = connection.CreateCommand();
        var scriptsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts/db.sql").ThrowIfFileNotExists();
        scriptCommand.CommandText = await File.ReadAllTextAsync(scriptsFile, ctoken);
        await scriptCommand.ExecuteNonQueryAsync(ctoken);
        
        SqliteConnection.ClearAllPools();
    }
}