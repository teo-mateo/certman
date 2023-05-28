using certman.CQRS.Queries;
using certman.Models;
using MediatR;

namespace certman.CQRS.Commands.CACerts;

public record PruneCACertsCommand() : IRequest<Unit>;

public class PruneCACertsCommandHandler : CertmanHandler<PruneCACertsCommand, Unit>
{
    private readonly IMediator _mediator;

    public PruneCACertsCommandHandler(IConfiguration config, IMediator mediator) : base(config)
    {
        _mediator = mediator;
    }

    protected override async Task<Unit> ExecuteAsync(PruneCACertsCommand request, CancellationToken ctoken)
    {
        var certs = await _mediator.Send(new GetAllCACertsQuery(), ctoken);
        foreach (var cert in certs)
        {
            await PruneCACert(cert);
        }

        return Unit.Value;
    }
    
    /// <summary>
    /// Verifies if the key and pem files exist, if not, deletes the cert from the db
    /// </summary>
    private async Task PruneCACert(CACert cert)
    {
        var keyFile = Path.Combine(_config["Store"], cert.Keyfile);
        var pemFile = Path.Combine(_config["Store"], cert.Pemfile);
        if (File.Exists(keyFile) && File.Exists(pemFile))
        {
            return;
        }
        
        File.Delete(keyFile);
        File.Delete(pemFile);
        
        await using var connection = await GetOpenConnectionAsync();
        
        // delete linked certs from db
        await using var deleteLinkedCertsCommand = connection.CreateCommand();
        deleteLinkedCertsCommand.CommandText = "DELETE FROM Certs WHERE CaCertId = @id";
        deleteLinkedCertsCommand.Parameters.AddWithValue("@id", cert.Id);
        await deleteLinkedCertsCommand.ExecuteNonQueryAsync();
        
        // delete CA cert from db
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM CACerts WHERE Id = @id";
        deleteCommand.Parameters.AddWithValue("@id", cert.Id);
        await deleteCommand.ExecuteNonQueryAsync();
    }
}