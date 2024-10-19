using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands.Certs;

public record DeleteLeafCertCommand(int CaCertId, int Id) : IRequest<Unit>;

public class DeleteLeafCertCommandHandler(IConfiguration config, ILogger<DeleteLeafCertCommandHandler> logger) : CertmanHandler<DeleteLeafCertCommand, Unit>(config, logger)
{
    protected override async Task<Unit> ExecuteAsync(DeleteLeafCertCommand request, CancellationToken ctoken)
    {
        logger.LogInformation("Deleting trusted cert with id {Id} and caCertId {CaCertId}", request.Id, request.CaCertId);
        
        await using var connection = await GetOpenConnectionAsync();

        var cert = await connection.QueryFirstOrDefaultAsync<Cert>(
            "SELECT * FROM Certs WHERE caCertId = @CaCertId AND id = @Id", 
            new { request.CaCertId, request.Id });
        
        if (cert == null)
            return Unit.Value;
        
        // delete all files of the leaf cert (Cert class) from the store
        File.Delete(Path.Combine(_config["Store"]!, cert.Csrfile));
        File.Delete(Path.Combine(_config["Store"]!, cert.Extfile));
        File.Delete(Path.Combine(_config["Store"]!, cert.Crtfile));
        File.Delete(Path.Combine(_config["Store"]!, cert.Pfxfile));
        File.Delete(Path.Combine(_config["Store"]!, cert.Keyfile));
        
        await connection.ExecuteAsync("DELETE FROM Certs WHERE Id = @Id", new { request.Id });
        return Unit.Value;
    }
}