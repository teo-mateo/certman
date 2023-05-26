using System.Text.Json;
using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Queries;

/// <summary>
/// Returns a CA Cert and all its signed certificates
/// </summary>
public record GetCACertQuery(int Id) : IRequest<CACert?>;

public class GetCACertQueryHandler : CertmanHandler<GetCACertQuery, CACert?>
{
    public GetCACertQueryHandler(IConfiguration config) : base(config) { }

    protected override async Task<CACert?> ExecuteAsync(GetCACertQuery request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnectionAsync();
        var caCert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Id = @id", new {id=request.Id});
        
        if (caCert == null)
            return null;

        var leafCerts = await connection.QueryAsync<Cert>("SELECT * FROM Certs WHERE CACertId = @id", new {id=request.Id});
        caCert.Certs = leafCerts;

        foreach (var c in caCert.Certs)
        {
            c.AltNames = JsonSerializer.Deserialize<AltNames>(c.AltNames.ToString()!)!;
        }
        
        return caCert;
    }
} 
