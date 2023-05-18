using certman.Controllers.Dto;
using certman.Models;
using certman.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

[Route("api/[controller]")]
public class CertsController : CertmanController
{
    private readonly IOpenSSL _ssl;
    private readonly ILogger<CertsController> _logger;

    //ctor
    public CertsController(IOpenSSL ssl, IConfiguration config, ILogger<CertsController> logger): base(config)
    {
        _ssl = ssl;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new CA Certificate
    /// </summary>
    [HttpPost("ca-certs")]
    public async Task<JsonResult> CreateCACert([FromBody] CreateCACertDto dto)
    {
        await using var connection = await GetOpenConnection();
        
        //return if cert already exists, checking the db by name
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Name = @name", new {name = dto.Name});
        if (cert != null)
        {
            await connection.CloseAsync();
            return new JsonResult(new {cert.Id, cert.Keyfile, cert.Pemfile});
        }

        // create the private key file with the OpenSSL class
        var keyfile = await _ssl.CreatePrivateKey(dto.Name);

        // create the PEM file with the OpenSSL class
        var pemfile = await _ssl.CreatePEMFile(keyfile);

        // insert cert into db
        await using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText =
            "INSERT INTO CACerts (Name, Keyfile, Pemfile, CreatedAt) VALUES (@name, @keyfile, @pemfile, @createdAt); SELECT last_insert_rowid();";
        insertCommand.Parameters.AddWithValue("@name", dto.Name);
        insertCommand.Parameters.AddWithValue("@keyfile", keyfile);
        insertCommand.Parameters.AddWithValue("@pemfile", pemfile);
        insertCommand.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
        var id = await insertCommand.ExecuteScalarAsync();

        await connection.CloseAsync();

        // move files from workdir to store
        var keyFile = Path.Combine(Config["Workdir"], keyfile);
        var keyFileDest = Path.Combine(Config["Store"], keyfile);
        System.IO.File.Move(keyFile, keyFileDest);
            
        var pemFile = Path.Combine(Config["Workdir"], pemfile);
        var pemFileDest = Path.Combine(Config["Store"], pemfile);
        System.IO.File.Move(pemFile, pemFileDest);
        
        return new JsonResult(new
        {
            Id = Convert.ToInt32(id), 
            Keyfile = keyfile, 
            Pemfile = pemfile
        });
    }
    
    /// <summary>
    /// Prune CA Certs
    /// </summary>
    [HttpDelete("ca-certs/prune")]
    public async Task<IActionResult> PruneCACerts()
    {
        var certs = await GetAllCACertsInternal();
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
        var certs = await GetAllCACertsInternal();
        return Ok(certs);
    }
    
    /// <summary>
    /// Downloads the key file part of a CA Cert
    /// </summary>
    [HttpGet("ca-certs/{id}/keyfile")]
    public async Task<IActionResult> DownloadCACertKeyfile(int id)
    {
        var cert = await GetCACert(id);
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
        var cert = await GetCACert(id);
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
        var cert = await GetCACert(id);
        if (cert == null)
        {
            return NotFound();
        }
        
        // copy the keyfile and pemfile of the CA Cert to the workdir
        var keyfileCA = CopyKeyfileToWorkdir(cert);
        var pemFileCA = CopyPemfileToWorkdir(cert);

        var (keyfile, csrfile) = await _ssl.CreateKeyAndCsr(dto.Name, new CsrInfo()
        {
            Country = dto.Country ?? "",
            State = dto.State ?? "",
            Locality = dto.Locality ?? "",
            Organization = dto.Organization ?? "",
            OrganizationUnit = dto.OrganizationUnit ?? "",
            CommonName = dto.CommonName ?? dto.Name
        });

        var extfile = await _ssl.CreateExtFile(dto.Name, dto.DnsNames, dto.IpAddresses);
        
        // create signed certificate using the private key, csr, and ext file
        var crtfile = await _ssl.CreateSelfSignedCert(
            dto.Name,
            keyfileCA,
            pemFileCA,
            System.IO.Path.Combine(Config["Workdir"], csrfile),
            System.IO.Path.Combine(Config["Workdir"], extfile));

        var pfxfile = await _ssl.BundleSelfSignedCert(dto.Name, keyfile, crtfile);
        return Ok();
    }

    private string CopyPemfileToWorkdir(CACert cert)
    {
        var pemFileCA = Path.Combine(Config["Workdir"], cert.Pemfile);
        System.IO.File.Copy(
            Path.Combine(Config["Store"], cert.Pemfile),
            pemFileCA);
        return pemFileCA;
    }

    private string CopyKeyfileToWorkdir(CACert cert)
    {
        var keyfileCA = Path.Combine(Config["Workdir"], cert.Keyfile);
        System.IO.File.Copy(
            Path.Combine(Config["Store"], cert.Keyfile),
            keyfileCA);
        return keyfileCA;
    }


    private async Task<CACert?> GetCACert(int id)
    {
        await using var connection = await GetOpenConnection();
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Id = @id", new {id});
        await connection.CloseAsync();
        return cert;
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
    
    private async Task<IEnumerable<CACert>> GetAllCACertsInternal()
    {
        await using var connection = await GetOpenConnection();
        
        // get all certs from db with dapper
        var certs = await connection.QueryAsync<CACert>("SELECT * FROM CACerts");
        await connection.CloseAsync();
        return certs!;
    }
}