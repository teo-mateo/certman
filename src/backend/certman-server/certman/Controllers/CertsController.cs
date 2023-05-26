using certman.Controllers.Dto;
using certman.CQRS.Commands;
using certman.CQRS.Commands.CACerts;
using certman.CQRS.Commands.Certs;
using certman.CQRS.Queries;
using certman.Extensions;
using certman.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

[Route("api/[controller]")]
public class CertsController : CertmanController
{
    private readonly ILogger<CertsController> _logger;
    private readonly IMediator _mediator;
    
    //ctor
    public CertsController(IConfiguration config, ILogger<CertsController> logger, IMediator mediator): base(config)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new CA Certificate
    /// </summary>
    [HttpPost("ca-certs")]
    public async Task<JsonResult> CreateCACert([FromBody] CreateCACertDto dto)
    {
        _logger.LogInformation("Creating CA Cert: {DtoName}", dto.Name);
        
        if (!ModelState.IsValid)
        {
            // return error and all ModelState errors
            return new JsonResult(ModelState.GetErrorMessages());
        }

        var result = await _mediator.Send(new CreateCACertCommand(dto));
        return new JsonResult(result);
    }
    
    /// <summary>
    /// Prune CA Certs
    /// </summary>
    [HttpDelete("ca-certs/prune")]
    public async Task<IActionResult> PruneCACerts()
    {
        _logger.LogInformation("Pruning CA Certs");
        
        await _mediator.Send(new PruneCACertsCommand());
        return Ok();
    }
    
    /// <summary>
    /// Gets all CA Certs
    /// </summary>
    [HttpGet("ca-certs")]
    public async Task<IActionResult> GetAllCACerts()
    {
        _logger.LogInformation("Getting all CA Certs");
        
        var certs = await _mediator.Send(new GetAllCACertsQuery());
        return Ok(certs);
    }

    /// <summary>
    /// Gets a CA Cert by Id
    /// </summary>
    [HttpGet("ca-certs/{id}")]
    public async Task<IActionResult> GetCACert(int id)
    {
        _logger.LogInformation("Getting CA Cert: {Id}", id);
        
        var caCert = await _mediator.Send(new GetCACertQuery(id));
        if (caCert == null)
            return NotFound();

        return Ok(caCert);
    }
    

    /// <summary>
    /// Downloads the key file part of a CA Cert
    /// </summary>
    [HttpGet("ca-certs/{id}/keyfile")]
    public async Task<IActionResult> DownloadCACertKeyfile(int id)
    {
        _logger.LogInformation("Downloading CA Cert Keyfile: {Id}", id);
        
        var cert = await _mediator.Send(new GetCACertQuery(id));
        if (cert == null)
        {
            return NotFound();
        }
        
        var keyFile = Path.Combine(Config["Store"], cert.Keyfile).ThrowIfFileNotExists();

        var stream = System.IO.File.OpenRead(keyFile);
        return File(stream, "application/octet-stream", cert.Keyfile);
    }
    
    /// <summary>
    ///  Downloads the pem file part of a CA Cert
    /// </summary>
    [HttpGet("ca-certs/{id}/pemfile")]
    public async Task<IActionResult> DownloadCACertPemfile(int id)
    {
        _logger.LogInformation("Downloading CA Cert Pemfile: {Id}", id);
        
        var cert = await _mediator.Send(new GetCACertQuery(id));
        if (cert == null)
        {
            return NotFound();
        }
        
        var pemFile = Path.Combine(Config["Store"], cert.Pemfile).ThrowIfFileNotExists();
        var stream = System.IO.File.OpenRead(pemFile);
        return File(stream, "application/octet-stream", cert.Pemfile);
    }

    [HttpGet("ca-certs/{caCertId}/certs/{id:int}/pfxfile")]
    public async Task<IActionResult> DownloadCertPfxFile(int caCertId, int id)
    {
        _logger.LogInformation("Downloading Cert Pfxfile: {Id}", id);
        
        var cert = await _mediator.Send(new GetTrustedCertQuery(caCertId, id));
        if (cert == null)
        {
            return NotFound();
        }

        var pfxFile = Path.Combine(Config["Store"], cert.Pfxfile).ThrowIfFileNotExists();
        await using var stream = System.IO.File.OpenRead(pfxFile);
        return File(stream, "application/octet-stream", cert.Pfxfile);
    }
    
    // get the key file of a trusted cert
    [HttpGet("ca-certs/{caCertId}/certs/{id}/keyfile")]
    public async Task<IActionResult> DownloadCertKeyFile(int caCertId, int id)
    {
        _logger.LogInformation("Downloading Cert Keyfile: {Id}", id);
        
        var cert = await _mediator.Send(new GetTrustedCertQuery(caCertId, id));
        if (cert == null)
        {
            return NotFound();
        }

        var keyFile = Path.Combine(Config["Store"], cert.Keyfile).ThrowIfFileNotExists();
        await using var stream = System.IO.File.OpenRead(keyFile);
        return File(stream, "application/octet-stream", cert.Keyfile);
    }
    
    /// <summary>
    /// Creates a new trusted certificate, signed by the CA Cert
    /// </summary>
    [HttpPost("ca-certs/{id}/certs")]
    public async Task<IActionResult> CreateTrustedCert(int id, [FromBody] CreateLeafCertDto dto)
    {
        _logger.LogInformation("Creating Trusted Cert: {DtoName}", dto.Name);
        
        if (!ModelState.IsValid)
        {
            // return error and all ModelState errors
            return new JsonResult(ModelState.GetErrorMessages());
        }
        
        var cert = await _mediator.Send(new CreateTrustedCertCommand(id, dto));
        return Ok(cert);
        
    }

    /// <summary>
    /// Deletes a trusted cert and all its files
    /// </summary>
    [HttpDelete("ca-certs/{caCertId}/certs/{id}")]
    public async Task<IActionResult> GetTrustedCert(int caCertId, int id)
    {
        _logger.LogInformation("Deleting Trusted Cert: {Id}", id);
        
        await _mediator.Send(new DeleteTrustedCertCommand(caCertId, id));
        return Ok();
    }

    /// <summary>
    /// Deletes a CA Cert including all linked certs
    /// </summary>
    [HttpDelete("ca-certs/{id}")]
    public async Task<IActionResult> DeleteCACert(int id)
    {
        _logger.LogInformation("Deleting CA Cert: {Id}", id);
        
        await _mediator.Send(new DeleteCACertCommand(id));
        return Ok();
    }
    

}