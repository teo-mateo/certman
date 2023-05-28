using MediatR;

namespace certman.CQRS.Commands.Startup;

public record EnsureCertificateExistsCommand : IRequest<Unit>;

public class EnsureCertificateExistsHandler : CertmanHandler<EnsureCertificateExistsCommand, Unit>
{
    public EnsureCertificateExistsHandler(IConfiguration config) : base(config)
    {
    }

    protected override Task<Unit> ExecuteAsync(EnsureCertificateExistsCommand request, CancellationToken ctoken)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        
        if(!File.Exists("Certman.pfx"))
            throw new FileNotFoundException($"Certman.pfx not found in [{currentDirectory}]. Please place it in the root directory of the application.");

        return Task.FromResult(Unit.Value);
    }
}