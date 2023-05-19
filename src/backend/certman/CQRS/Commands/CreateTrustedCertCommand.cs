using System.Text.Json;
using certman.Controllers.Dto;
using certman.CQRS.Queries;
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

        var extFile = await _ssl.CreateExtFile(request.Dto.Name, request.Dto.DnsNames, request.Dto.IpAddresses);
        
        // create signed certificate using the private key, csr, and ext file
        var crtFile = await _ssl.CreateSelfSignedCert(
            request.Dto.Name,
            keyFileCA,
            pemFileCA,
            System.IO.Path.Combine(Config["Workdir"], csrFile),
            System.IO.Path.Combine(Config["Workdir"], extFile));

        var pfxFile = await _ssl.BundleSelfSignedCert(request.Dto.Name, keyFile, crtFile, request.Dto.Password);
        
         
        // object with request.dto.DnsNames and request.dto.IpAddresses
        var altNames = new
        {
            request.Dto.DnsNames,
            request.Dto.IpAddresses
        };
        var altNamesJson = JsonSerializer.Serialize(altNames);

        var cert = new Cert()
        {
            AltNames = altNamesJson,
            CaCertId = request.Id,
            Name = request.Dto.Name,
            CreatedAt = DateTime.Now,
            Csrfile = csrFile,
            Extfile = extFile,
            Keyfile = keyFile,
            Password = request.Dto.Password,
            Pfxfile = pfxFile
        };

        // insert cert into db
        await using var connection = await GetOpenConnection();
        var id = await connection.ExecuteScalarAsync<int>(
            "INSERT INTO Certs (CaCertId, Name, Keyfile, Csrfile, Extfile, Pfxfile, Password, CreatedAt) VALUES (@CaCertId, @Name, @Keyfile, @Csrfile, @Extfile, @Pfxfile, @Password, @CreatedAt); SELECT last_insert_rowid();",
            cert);

        await connection.CloseAsync();

        return (await _mediator.Send(new GetTrustedCertQuery(id), ctoken))!;
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
}