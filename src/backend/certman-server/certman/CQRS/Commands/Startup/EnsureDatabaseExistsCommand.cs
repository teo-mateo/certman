using certman.CQRS.Commands.Storage;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS.Commands.Startup;

public record EnsureDatabaseExistsCommand : IRequest<Unit>;

public class EnsureDatabaseExistsHandler : CertmanHandler<EnsureDatabaseExistsCommand, Unit>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnsureDatabaseExistsHandler> _logger;

    public EnsureDatabaseExistsHandler(IConfiguration config, IMediator mediator, ILogger<EnsureDatabaseExistsHandler> logger) : base(config)
    {
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task<Unit> ExecuteAsync(EnsureDatabaseExistsCommand request, CancellationToken ctoken)
    {
        var connectionString = $"Data Source={Config["Database"]}";
        var databaseFile = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (File.Exists(databaseFile))
        {
            _logger.LogInformation("Database exists: {DatabaseFile}", databaseFile);
            return Unit.Value;
        }
        
        _logger.LogInformation("Database does not exist, creating {DatabaseFile}...", databaseFile);
        await _mediator.Send(new RecreateDbTablesCommand(), ctoken);
        return Unit.Value;
    }
}