﻿using certman.Models;
using Heapzilla.Common.Filesystem;
using MediatR;

namespace certman.CQRS.Queries;

public record GetLeafCertFileStreamQuery(
    int CaCertId,
    int LeafCertId,
    Func<Cert, string> fileFunc) : IRequest<(string, FileStream)>;

public class GetLeafCertFileStreamHandler : CertmanHandler<GetLeafCertFileStreamQuery, (string, FileStream)>
{
    private readonly IMediator _mediator;

    public GetLeafCertFileStreamHandler(
        IConfiguration config,
        IMediator mediator,
        ILogger<GetLeafCertFileStreamQuery> logger) : base(config, logger)
    {
        _mediator = mediator;
    }

    protected override async Task<(string, FileStream)> ExecuteAsync(GetLeafCertFileStreamQuery request, CancellationToken ctoken)
    {
        _logger?.LogInformation("Downloading Leaf Cert Pemfile: {Id}", request.LeafCertId);
        
        var cert = await _mediator.Send(new GetLeafCertQuery(request.CaCertId, request.LeafCertId), ctoken);
        if (cert == null)
        {
            throw new FileNotFoundException();
        }
        
        var file = request.fileFunc(cert);
        var filePath = Path.Combine(_config["Store"], file).ThrowIfFileNotExists();
        var stream = System.IO.File.OpenRead(filePath);
        return (file, stream);
    }
}