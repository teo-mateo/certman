using certman.CQRS.Commands.Certs;
using certman.CQRS.Queries;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands.CACerts;

public record DeleteCACertCommand(int Id) : IRequest<Unit>;

// handler
public class DeleteCACertCommandHandler : CertmanHandler<DeleteCACertCommand, Unit>
{
    private readonly IMediator _mediator;
    public DeleteCACertCommandHandler(IConfiguration config, IMediator mediator) : base(config)
    {
        _mediator = mediator;
    }

    protected override async Task<Unit> ExecuteAsync(DeleteCACertCommand request, CancellationToken ctoken)
    {
        // get the CA cert
        var caCert = await _mediator.Send(new GetCACertQuery(request.Id), ctoken);
        if (caCert == null)
            return Unit.Value;
        
        // delete all files of the CA cert (CACert class) from the store
        File.Delete(Path.Combine(Config["Store"], caCert.Pemfile));
        File.Delete(Path.Combine(Config["Store"], caCert.Keyfile));

        foreach (var cert in caCert.Certs)
        {
            await _mediator.Send(new DeleteTrustedCertCommand(caCert.Id, cert.Id), ctoken);
        }
        
        await using var connection = await GetOpenConnectionAsync();
        await connection.ExecuteAsync("DELETE FROM CACerts WHERE Id = @Id", new { request.Id });
        return Unit.Value;
    }
}