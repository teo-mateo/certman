using System.Text.Json;
using certman.Controllers.Dto;
using certman.CQRS.Queries;
using certman.Extensions;
using certman.Models;
using certman.Services;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands;

public record CreateTrustedCertCommand(int Id, CreateTrustedCertDto Dto) : IRequest<Cert>;
public class CreateTrustedCertCommandHandler : CertmanHandler<CreateTrustedCertCommand, Cert>
{
    private readonly IMediator _mediator;
    private readonly IOpenSSL _ssl;

    public CreateTrustedCertCommandHandler(IConfiguration config, IMediator mediator, IOpenSSL ssl) : base(config)
    {
        _mediator = mediator;
        _ssl = ssl;
    }

    protected override async Task<Cert> ExecuteAsync(CreateTrustedCertCommand request, CancellationToken ctoken)
    {
        var caCert = await _mediator.Send(new GetCACertQuery(request.Id), ctoken);
        if (caCert == null)
        {
            throw new Exception("CA Cert not found");
        }
        
        await ThrowIfCertWithNameAlreadyExists(request.Dto.Name);

        await _mediator.Send(new ClearWorkdirCommand(), ctoken);
        
        // copy the keyfile and pemfile of the CA Cert to the workdir
        var keyFileCA = CopyKeyfileToWorkdir(caCert);
        var pemFileCA = CopyPemfileToWorkdir(caCert);

        var (keyFile, csrFile) = await _ssl.CreateKeyAndCsr(request.Dto.Name, new CsrInfo()
        {
            Country = request.Dto.Country ?? "",
            State = request.Dto.State ?? "",
            Locality = request.Dto.Locality ?? "",
            Organization = request.Dto.Organization ?? "",
            OrganizationUnit = request.Dto.OrganizationUnit ?? "",
            CommonName = request.Dto.CommonName ?? request.Dto.Name
        });

        var extFile = await _ssl.CreateExtFile(
            request.Dto.Name, 
            request.Dto.DnsNames ?? Array.Empty<string>(), 
            request.Dto.IpAddresses ?? Array.Empty<string>());
        
        // create signed certificate using the private key, csr, and ext file
        var crtFile = await _ssl.CreateSelfSignedCert(
            request.Dto.Name,
            keyFileCA,
            pemFileCA,
            System.IO.Path.Combine(Config["Workdir"], csrFile),
            System.IO.Path.Combine(Config["Workdir"], extFile));

        var pfxFile = await _ssl.BundleSelfSignedCert(request.Dto.Name, keyFile, crtFile, request.Dto.Password);
        
         
        // object with request.dto.DnsNames and request.dto.IpAddresses
        var altNames = JsonSerializer.Serialize(new AltNames()
        {
            DnsNames = request.Dto.DnsNames ?? Array.Empty<string>(),
            IpAddresses = request.Dto.IpAddresses ?? Array.Empty<string>()
        });

        var dbParameters = new
        {
            CaCertId = request.Id,
            Name = request.Dto.Name,
            altNames,
            keyFile,
            csrFile,
            extFile,
            pfxFile,
            request.Dto.Password,
            CreatedAt = DateTime.Now
        };
        
        // insert cert into db
        await using var connection = await GetOpenConnection();
        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"INSERT INTO Certs (caCertId, Name, altNames, keyfile, csrfile, extfile, pfxfile, password, createdAt) 
                 VALUES (@CaCertId, @Name, @altNames, @keyfile, @csrfile, @extfile, @pfxfile, @Password, @CreatedAt); 
                 SELECT last_insert_rowid();";
        insertCommand.Parameters.AddWithValue("@CaCertId", request.Id);
        insertCommand.Parameters.AddWithValue("@Name", request.Dto.Name);
        insertCommand.Parameters.AddWithValue("@altNames", altNames);
        insertCommand.Parameters.AddWithValue("@keyfile", keyFile);
        insertCommand.Parameters.AddWithValue("@csrfile", csrFile);
        insertCommand.Parameters.AddWithValue("@extfile", extFile);
        insertCommand.Parameters.AddWithValue("@pfxfile", pfxFile);
        insertCommand.Parameters.AddWithValue("@Password", request.Dto.Password);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        var id = Convert.ToInt32(await insertCommand.ExecuteScalarAsync(ctoken));
        await connection.CloseAsync();
        
        return (await _mediator.Send(new GetTrustedCertQuery(id), ctoken))!;
    }
    
    private string CopyPemfileToWorkdir(CACert cert)
    {
        var pemFileCA = Path.Combine(Config["Workdir"], cert.Pemfile);
        System.IO.File.Copy(
            Path.Combine(Config["Store"], cert.Pemfile).ThrowIfFileNotExists(),
            pemFileCA);
        return pemFileCA;
    }

    private string CopyKeyfileToWorkdir(CACert cert)
    {
        var keyfileCA = Path.Combine(Config["Workdir"], cert.Keyfile);
        System.IO.File.Copy(
            Path.Combine(Config["Store"], cert.Keyfile).ThrowIfFileNotExists(),
            keyfileCA);
        return keyfileCA;
    }

    private async Task ThrowIfCertWithNameAlreadyExists(string name)
    {
        var connection = await GetOpenConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Certs WHERE Name = @name", 
            new { name });
        
        if (count > 0)
            throw new Exception($"Cert with name {name} already exists");
    }
}