using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS;

public abstract class CertmanHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly IConfiguration Config;

    protected CertmanHandler(IConfiguration config)
    {
        Config = config;
    }

    protected abstract Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ctoken);

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        return this.ExecuteAsync(request, cancellationToken);
    }
    
    protected async Task<SqliteConnection> GetOpenConnectionAsync()
    {
        //get connection string
        var connectionString = Config.GetConnectionString("DefaultConnection");

        //open connection
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
    
    protected SqliteConnection GetOpenConnection()
    {
        //get connection string
        var connectionString = Config.GetConnectionString("DefaultConnection");

        //open connection
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}