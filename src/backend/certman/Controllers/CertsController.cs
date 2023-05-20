using certman.Controllers.Dto;
using certman.CQRS.Commands;
using certman.CQRS.Queries;
using certman.Extensions;
using certman.Models;
using certman.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

[Route("api/[controller]")]
public class CertsController : CertmanController
{
    private readonly IOpenSSL _ssl;
    private readonly ILogger<CertsController> _logger;
    private readonly IMediator _mediator;

    //ctor
    public CertsController(IOpenSSL ssl, IConfiguration config, ILogger<CertsController> logger, IMediator mediator): base(config)
    {
        _ssl = ssl;
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new CA Certificate
    /// </summary>
    [HttpPost("ca-certs")]
    public async Task<JsonResult> CreateCACert([FromBody] CreateCACertDto dto)
    {
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
        var certs = await _mediator.Send(new GetAllCACertsQuery());
        foreach (var cert in certs)
        {
            await PruneCACert(cert);
        }
        return Ok();
    }
    
    /// <summary>
    /// Gets all CA Certs
    /// </summary>
    [HttpGet("ca-certs")]
    public async Task<IActionResult> GetAllCACerts()
    {
        var certs = await _mediator.Send(new GetAllCACertsQuery());
        return Ok(certs);
    }

    /// <summary>
    /// Gets a CA Cert by Id
    /// </summary>
    [HttpGet("ca-certs/{id}")]
    public async Task<IActionResult> GetCACert(int id)
    {
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
        var cert = await _mediator.Send(new GetCACertQuery(id));
        if (cert == null)
        {
            return NotFound();
        }
        
        var keyFile = Path.Combine(Config["Store"], cert.Keyfile);
        if (!System.IO.File.Exists(keyFile))
        {
            return NotFound();
        }
        
        var stream = System.IO.File.OpenRead(keyFile);
        return File(stream, "application/octet-stream", cert.Keyfile);
    }
    
    /// <summary>
    ///  Downloads the pem file part of a CA Cert
    /// </summary>
    [HttpGet("ca-certs/{id}/pemfile")]
    public async Task<IActionResult> DownloadCACertPemfile(int id)
    {
        var cert = await _mediator.Send(new GetCACertQuery(id));
        if (cert == null)
        {
            return NotFound();
        }
        
        var pemFile = Path.Combine(Config["Store"], cert.Pemfile);
        if (!System.IO.File.Exists(pemFile))
        {
            return NotFound();
        }
        
        var stream = System.IO.File.OpenRead(pemFile);
        return File(stream, "application/octet-stream", cert.Pemfile);
    }
    
    /// <summary>
    /// Creates a new trusted certificate, signed by the CA Cert
    /// </summary>
    [HttpPost("ca-certs/{id}/certs")]
    public async Task<IActionResult> CreateTrustedCert(int id, [FromBody] CreateTrustedCertDto dto)
    {
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
        await _mediator.Send(new DeleteTrustedCertCommand(id));
        return Ok();
    }

    /// <summary>
    /// Deletes a CA Cert including all linked certs
    /// </summary>
    [HttpDelete("ca-certs/{id}")]
    public async Task<IActionResult> DeleteCACert(int id)
    {
        await _mediator.Send(new DeleteCACertCommand(id));
        return Ok();
    }
    
    /// <summary>
    /// Verifies if the key and pem files exist, if not, deletes the cert from the db
    /// </summary>
    private async Task PruneCACert(CACert cert)
    {
        var keyFile = Path.Combine(Config["Store"], cert.Keyfile);
        var pemFile = Path.Combine(Config["Store"], cert.Pemfile);
        if (System.IO.File.Exists(keyFile) && System.IO.File.Exists(pemFile))
        {
            return;
        }
        
        System.IO.File.Delete(keyFile);
        System.IO.File.Delete(pemFile);
        
        await using var connection = await GetOpenConnection();
        
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
        
        // close connection
        await connection.CloseAsync();
    }
}