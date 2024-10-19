using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Queries;

public record GetLeafCertQuery(int CaCertId, int LeafCertId) : IRequest<Cert?>;

public class GetCertQueryHandler(IConfiguration config, ILogger<GetCertQueryHandler> logger) : CertmanHandler<GetLeafCertQuery, Cert?>(config, logger)
{
    protected override async Task<Cert?> ExecuteAsync(GetLeafCertQuery request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnectionAsync();
        var cert = await connection.QueryFirstOrDefaultAsync<Cert>(
            "SELECT * FROM Certs WHERE CACertId = @CaCertId AND Id = @Id", 
            new
            {
                request.CaCertId, 
                Id = request.LeafCertId
            });
        await connection.CloseAsync();
        return cert; 
    }
}