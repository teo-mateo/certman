using System.Text.Json;
using certman.Controllers.Dto;
using certman.CQRS.Commands.Storage;
using certman.CQRS.Queries;
using certman.Extensions;
using certman.Models;
using certman.Services;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands.Certs;

public record CreateLeafCertCommand(int CaCertId, CreateLeafCertDto Dto) : IRequest<Cert>;

public class CreateLeafCertCommandHandler : CertmanHandler<CreateLeafCertCommand, Cert>
{
    private readonly IMediator _mediator;
    private readonly IOpenSSL _ssl;

    public CreateLeafCertCommandHandler(IConfiguration config, IMediator mediator, IOpenSSL ssl) : base(config)
    {
        _mediator = mediator;
        _ssl = ssl;
    }

    protected override async Task<Cert> ExecuteAsync(CreateLeafCertCommand request, CancellationToken ctoken)
    {
        var caCert = await _mediator.Send(new GetCACertQuery(request.CaCertId), ctoken);
        if (caCert == null)
        {
            throw new Exception("CA Cert not found");
        }
        
        await ThrowIfCertWithNameAlreadyExists(request.Dto.Name);

        await _mediator.Send(new ClearWorkdirCommand(), ctoken);
        
        // copy the keyfile and pemfile of the CA Cert to the workdir
        var keyFileCA = CopyFromStoreToWorkdir(caCert.Keyfile);
        var pemFileCA = CopyFromStoreToWorkdir(caCert.Pemfile);

        var (keyFile, csrFile) = await _ssl.CreateKeyAndCsr(
            request.Dto.Name,
            new CsrInfo
            {
                Country = request.Dto.Country ?? "",
                State = request.Dto.State ?? "",
                Locality = request.Dto.Locality ?? "",
                Organization = request.Dto.Organization ?? "",
                OrganizationUnit = request.Dto.OrganizationUnit ?? "",
                CommonName = request.Dto.CommonName
            });

        var extFile = await _ssl.CreateExtFile(
            request.Dto.Name, 
            request.Dto.DnsNames, 
            request.Dto.IpAddresses ?? Array.Empty<string>());
        
        // create signed certificate using the private key, csr, and ext file
        var crtFile = await _ssl.CreateSignedCert(
            request.Dto.Name,
            keyFileCA,
            pemFileCA,
            Path.Combine(_config["Workdir"], csrFile),
            Path.Combine(_config["Workdir"], extFile));

        var pfxFile = await _ssl.BundleSelfSignedCert(request.Dto.Name, keyFile, crtFile, request.Dto.Password);

        // object with request.dto.DnsNames and request.dto.IpAddresses
        var altNames = JsonSerializer.Serialize(
            new AltNames
            {
                DnsNames = request.Dto.DnsNames,
                IpAddresses = request.Dto.IpAddresses ?? Array.Empty<string>()
            });

        // insert cert into db
        await using var connection = await GetOpenConnectionAsync();
        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"INSERT INTO Certs (caCertId, Name, altNames, keyfile, csrfile, extfile, crtfile, pfxfile, password, createdAt) 
                 VALUES (@CaCertId, @Name, @altNames, @keyfile, @csrfile, @extfile, @crtfile, @pfxfile, @Password, @CreatedAt); 
                 SELECT last_insert_rowid();";
        insertCommand.Parameters.AddWithValue("@CaCertId", request.CaCertId);
        insertCommand.Parameters.AddWithValue("@Name", request.Dto.Name);
        insertCommand.Parameters.AddWithValue("@altNames", altNames);
        insertCommand.Parameters.AddWithValue("@keyfile", keyFile);
        insertCommand.Parameters.AddWithValue("@csrfile", csrFile);
        insertCommand.Parameters.AddWithValue("@extfile", extFile);
        insertCommand.Parameters.AddWithValue("@crtfile", crtFile);
        insertCommand.Parameters.AddWithValue("@pfxfile", pfxFile);
        insertCommand.Parameters.AddWithValue("@Password", request.Dto.Password);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        var id = Convert.ToInt32(await insertCommand.ExecuteScalarAsync(ctoken));
        await connection.CloseAsync();

        MoveFromWorkdirToStore(keyFile);
        MoveFromWorkdirToStore(csrFile);
        MoveFromWorkdirToStore(extFile);
        MoveFromWorkdirToStore(crtFile);
        MoveFromWorkdirToStore(pfxFile);
        
        //cleanup workdir
        await _mediator.Send(new ClearWorkdirCommand(), ctoken);
        
        return (await _mediator.Send(new GetLeafCertQuery(request.CaCertId, id), ctoken))!;
    }

    private async Task ThrowIfCertWithNameAlreadyExists(string name)
    {
        await using var connection = await GetOpenConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Certs WHERE Name = @name", 
            new { name });
        
        if (count > 0)
            throw new Exception($"Cert with name {name} already exists");
    }
    
    private void MoveFromWorkdirToStore(string file)
    {
        var source = Path.Combine(_config["Workdir"], file).ThrowIfFileNotExists();
        var dest = Path.Combine(_config["Store"], file).ThrowIfFileExists();
        File.Move(source, dest);
    }

    private string CopyFromStoreToWorkdir(string file)
    {
        var source = Path.Combine(_config["Store"], file).ThrowIfFileNotExists();
        var dest = Path.Combine(_config["Workdir"], file).ThrowIfFileExists();
        File.Copy(source, dest);
        return dest;
    }
}