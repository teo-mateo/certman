using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using certman.Extensions;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS;

public abstract class CertmanHandler<TRequest, TResponse>(IConfiguration config, ILogger logger)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly IConfiguration _config = config;

    protected abstract Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ctoken);

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing {Request}", request.GetType().Name);
        
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