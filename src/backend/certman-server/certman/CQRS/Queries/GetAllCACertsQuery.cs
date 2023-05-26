using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Queries;

public record GetAllCACertsQuery: IRequest<CACert[]>;

public class GetAllCACertsQueryHandler : CertmanHandler<GetAllCACertsQuery, CACert[]>
{
    public GetAllCACertsQueryHandler(IConfiguration config) : base(config) { }

    protected override async Task<CACert[]> ExecuteAsync(GetAllCACertsQuery request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnectionAsync();
        var certs = await connection.QueryAsync<CACert>("SELECT * FROM CACerts");
        return certs.ToArray();
    }
}