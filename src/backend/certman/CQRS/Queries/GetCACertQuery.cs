using System.Text.Json;
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
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Id = @id", new {id=request.Id});
        
        if (cert == null)
            return null;

        var certs = await connection.QueryAsync<Cert>("SELECT * FROM Certs WHERE CACertId = @id", new {id=request.Id});
        cert.Certs = certs;

        foreach (var c in cert.Certs)
        {
            // deserialize as AltNames to the same field
            c.AltNames = JsonSerializer.Deserialize<AltNames>(c.AltNames.ToString()!)!;
        }
        
        return cert;
    }
} 
