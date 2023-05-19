using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Queries;

public record GetTrustedCertQuery(int Id) : IRequest<Cert?>;

public class GetCertQueryHandler: CertmanHandler<GetTrustedCertQuery, Cert?>
{
    public GetCertQueryHandler(IConfiguration config) : base(config) { }
    
    protected override async Task<Cert?> ExecuteAsync(GetTrustedCertQuery request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnection();
        var cert = await connection.QueryFirstOrDefaultAsync<Cert>("SELECT * FROM Certs WHERE Id = @id", new {request.Id});
        await connection.CloseAsync();
        return cert; 
    }
}