using certman.CQRS.Commands.Certs;
using certman.CQRS.Queries;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands.CACerts;

public record DeleteCACertCommand(int Id) : IRequest<Unit>;

// handler
public class DeleteCACertCommandHandler(IConfiguration config, IMediator mediator, ILogger<DeleteCACertCommandHandler> logger)
    : CertmanHandler<DeleteCACertCommand, Unit>(config, logger)
{
    protected override async Task<Unit> ExecuteAsync(DeleteCACertCommand request, CancellationToken ctoken)
    {
        // get the CA cert
        var caCert = await mediator.Send(new GetCACertQuery(request.Id), ctoken);
        if (caCert == null)
            return Unit.Value;
        
        logger?.LogInformation("Deleting CA cert with id {Id} and name {Name}", caCert.Id, caCert.Name);
        
        // delete all files of the CA cert (CACert class) from the store
        File.Delete(Path.Combine(_config["Store"]!, caCert.Pemfile));
        File.Delete(Path.Combine(_config["Store"]!, caCert.Keyfile));

        foreach (var cert in caCert.Certs)
        {
            await mediator.Send(new DeleteLeafCertCommand(caCert.Id, cert.Id), ctoken);
        }
        
        await using var connection = await GetOpenConnectionAsync();
        await connection.ExecuteAsync("DELETE FROM CACerts WHERE Id = @Id", new { request.Id });
        return Unit.Value;
    }
}