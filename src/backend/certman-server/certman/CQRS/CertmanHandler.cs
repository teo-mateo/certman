using certman.Extensions;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS;

public abstract class CertmanHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly IConfiguration _config;
    protected readonly ILogger? _logger;

    protected CertmanHandler(IConfiguration config, ILogger? logger = null)
    {
        _config = config;
        _logger = logger;
    }

    protected abstract Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ctoken);

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        return this.ExecuteAsync(request, cancellationToken);
    }
    
    protected async Task<SqliteConnection> GetOpenConnectionAsync()
    {
        //get connection string
        var connectionString = _config.GetConnectionString();

        //open connection
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}