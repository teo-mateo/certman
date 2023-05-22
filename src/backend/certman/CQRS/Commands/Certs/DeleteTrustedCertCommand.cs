using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands.Certs;

public record DeleteTrustedCertCommand(int CaCertId, int Id) : IRequest<Unit>;

public class DeleteTrustedCertCommandHandler : CertmanHandler<DeleteTrustedCertCommand, Unit>
{
    public DeleteTrustedCertCommandHandler(IConfiguration config) : base(config) { }

    protected override async Task<Unit> ExecuteAsync(DeleteTrustedCertCommand request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnectionAsync();

        var cert = await connection.QueryFirstOrDefaultAsync<Cert>(
            "SELECT * FROM Certs WHERE caCertId = @CaCertId AND id = @Id", 
            new { request.CaCertId, request.Id });
        
        if (cert == null)
            return Unit.Value;
        
        // delete all files of the trusted cert (Cert class) from the store
        File.Delete(Path.Combine(Config["Store"], cert.Csrfile));
        File.Delete(Path.Combine(Config["Store"], cert.Extfile));
        File.Delete(Path.Combine(Config["Store"], cert.Pfxfile));
        File.Delete(Path.Combine(Config["Store"], cert.Keyfile));
        
        await connection.ExecuteAsync("DELETE FROM Certs WHERE Id = @Id", new { request.Id });
        return Unit.Value;
    }
}