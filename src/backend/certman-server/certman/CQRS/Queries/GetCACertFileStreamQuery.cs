﻿using certman.Extensions;
using certman.Models;
using MediatR;

namespace certman.CQRS.Queries;

public record GetCACertFileStreamQuery(
    int CaCertId, 
    Func<CACert, string> fileFunc) : IRequest<(string, FileStream)>;

public class GetCACertFileStreamHandler : CertmanHandler<GetCACertFileStreamQuery, (string, FileStream)>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetCACertFileStreamQuery> _logger;

    public GetCACertFileStreamHandler(
        IConfiguration config,
        IMediator mediator,
        ILogger<GetCACertFileStreamQuery> logger) : base(config)
    {
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task<(string, FileStream)> ExecuteAsync(GetCACertFileStreamQuery request, CancellationToken ctoken)
    {
        _logger.LogInformation("Downloading CA Cert Pemfile: {Id}", request.CaCertId);
        
        var cert = await _mediator.Send(new GetCACertQuery(request.CaCertId), ctoken);
        if (cert == null)
        {
            throw new FileNotFoundException();
        }
        
        var file = request.fileFunc(cert);
        var filePath = Path.Combine(Config["Store"], file).ThrowIfFileNotExists();
        var stream = System.IO.File.OpenRead(filePath);
        return (file, stream);
    }
}