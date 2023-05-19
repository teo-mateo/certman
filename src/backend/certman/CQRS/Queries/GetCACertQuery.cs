using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Queries;

public record GetCACertQuery(int Id) : IRequest<CACert?>;

public class GetCACertQueryHandler : CertmanHandler<GetCACertQuery, CACert?>
{
    public GetCACertQueryHandler(IConfiguration config) : base(config) { }

    protected override async Task<CACert?> ExecuteAsync(GetCACertQuery request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnection();
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Id = @id", new {request.Id});
        
        if (cert == null)
            return null;

        var certs = await connection.QueryAsync<Cert>("SELECT * FROM Certs WHERE CACertId = @id", new {request.Id});
        cert.Certs = certs;
        
        await connection.CloseAsync();
        return cert;
    }
} 