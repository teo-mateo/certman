using certman.Models;
using Heapzilla.Common.Filesystem;
using MediatR;

namespace certman.CQRS.Queries;

public record GetCACertFileStreamQuery(
    int CaCertId, 
    Func<CACert, string> fileFunc) : IRequest<(string, FileStream)>;

public class GetCACertFileStreamHandler(
    IConfiguration config,
    IMediator mediator,
    ILogger<GetCACertFileStreamQuery> logger)
    : CertmanHandler<GetCACertFileStreamQuery, (string, FileStream)>(config, logger)
{
    protected override async Task<(string, FileStream)> ExecuteAsync(GetCACertFileStreamQuery request, CancellationToken ctoken)
    {
        logger?.LogInformation("Downloading CA Cert Pemfile: {Id}", request.CaCertId);
        
        var cert = await mediator.Send(new GetCACertQuery(request.CaCertId), ctoken);
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