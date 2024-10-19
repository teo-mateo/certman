using certman.CQRS.Commands.Storage;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS.Commands.Startup;

public record EnsureDatabaseExistsCommand : IRequest<Unit>;

public class EnsureDatabaseExistsHandler(
    IConfiguration config,
    IMediator mediator,
    ILogger<EnsureDatabaseExistsHandler> logger)
    : CertmanHandler<EnsureDatabaseExistsCommand, Unit>(config, logger)
{
    protected override async Task<Unit> ExecuteAsync(EnsureDatabaseExistsCommand request, CancellationToken ctoken)
    {
        var connectionString = $"Data Source={_config["Database"]}";
        var databaseFile = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (File.Exists(databaseFile))
        {
            logger.LogInformation("Database exists: {DatabaseFile}", databaseFile);
            return Unit.Value;
        }
        
        logger.LogInformation("Database does not exist, creating {DatabaseFile}...", databaseFile);
        await mediator.Send(new RecreateDbTablesCommand(), ctoken);
        return Unit.Value;
    }
}