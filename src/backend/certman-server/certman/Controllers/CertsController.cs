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
        var (file, stream) = await _mediator.Send(new GetCACertFileStreamQuery(id, c => c.Keyfile));
        return File(stream, "application/octet-stream", file);
    }
    
    /// <summary>
    ///  Downloads the pem file part of a CA Cert
    /// </summary>
    [HttpGet("ca-certs/{id}/pemfile")]
    public async Task<IActionResult> DownloadCACertPemfile(int id)
    {
        var (file, stream) = await _mediator.Send(new GetCACertFileStreamQuery(id, c => c.Pemfile));
        return File(stream, "application/octet-stream", file);
    }

    /// <summary>
    /// Downloads the pfx file of a Leaf Cert
    /// </summary>
    [HttpGet("ca-certs/{caCertId}/certs/{id:int}/pfxfile")]
    public async Task<IActionResult> DownloadCertPfxFile(int caCertId, int id)
    {
        var (file, stream) = await _mediator.Send(new GetLeafCertFileStreamQuery(caCertId, id, c => c.Pfxfile));
        return File(stream, "application/octet-stream", file);
    }
    
    /// <summary>
    /// Downloads the key file of a Leaf Cert
    /// </summary>
    [HttpGet("ca-certs/{caCertId}/certs/{id}/keyfile")]
    public async Task<IActionResult> DownloadCertKeyFile(int caCertId, int id)
    {
        var (file, stream) = await _mediator.Send(new GetLeafCertFileStreamQuery(caCertId, id, c => c.Keyfile));
        return File(stream, "application/octet-stream", file);
    }
    
    /// <summary>
    /// Downloads the crt file of a Leaf Cert
    /// </summary>
    [HttpGet("ca-certs/{caCertId}/certs/{id}/crtfile")]
    public async Task<IActionResult> DownloadCertCrtFile(int caCertId, int id)
    {
        var (file, stream) = await _mediator.Send(new GetLeafCertFileStreamQuery(caCertId, id, c => c.Crtfile));
        return File(stream, "application/octet-stream", file);
    }
    
    /// <summary>
    /// Creates a new leaf certificate, signed by the CA Cert
    /// </summary>
    [HttpPost("ca-certs/{id}/certs")]
    public async Task<IActionResult> CreateLeafCert(int id, [FromBody] CreateLeafCertDto dto)
    {
        _logger.LogInformation("Creating Leaf Cert: {DtoName}", dto.Name);
        
        if (!ModelState.IsValid)
        {
            // return error and all ModelState errors
            return new JsonResult(ModelState.GetErrorMessages());
        }
        
        var cert = await _mediator.Send(new CreateLeafCertCommand(id, dto));
        return Ok(cert);
        
    }

    /// <summary>
    /// Deletes a leaf cert and all its files
    /// </summary>
    [HttpDelete("ca-certs/{caCertId}/certs/{id}")]
    public async Task<IActionResult> GetLeafCert(int caCertId, int id)
    {
        _logger.LogInformation("Deleting Leaf Cert: {Id}", id);
        
        await _mediator.Send(new DeleteLeafCertCommand(caCertId, id));
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