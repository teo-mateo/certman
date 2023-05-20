using certman.CQRS.Queries;
using certman.Models;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands;

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
            await _mediator.Send(new DeleteTrustedCertCommand(cert.Id), ctoken);
        }
        
        await using var connection = await GetOpenConnection();
        await connection.ExecuteAsync("DELETE FROM CACerts WHERE Id = @id", new {id=request.Id});
        return Unit.Value;
    }
}