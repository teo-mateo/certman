using certman.Extensions;
using certman.Models;
using MediatR;

namespace certman.CQRS.Queries;

public record GetLeafCertFileStreamQuery(
    int CaCertId,
    int LeafCertId,
    Func<Cert, string> fileFunc) : IRequest<(string, FileStream)>;

public class GetLeafCertFileStreamHandler(
    IConfiguration config,
    IMediator mediator,
    ILogger<GetLeafCertFileStreamQuery> logger)
    : CertmanHandler<GetLeafCertFileStreamQuery, (string, FileStream)>(config, logger)
{
    protected override async Task<(string, FileStream)> ExecuteAsync(GetLeafCertFileStreamQuery request, CancellationToken ctoken)
    {
        logger?.LogInformation("Downloading Leaf Cert Pemfile: {Id}", request.LeafCertId);
        
        var cert = await mediator.Send(new GetLeafCertQuery(request.CaCertId, request.LeafCertId), ctoken);
        if (cert == null)
        {
            throw new FileNotFoundException();
        }
        
        var file = request.fileFunc(cert);
        var filePath = Path.Combine(_config["Store"]!, file).ThrowIfFileNotExists();
        var stream = System.IO.File.OpenRead(filePath);
        return (file, stream);
    }
}