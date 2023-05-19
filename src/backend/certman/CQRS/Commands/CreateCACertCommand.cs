using certman.Controllers.Dto;
using certman.CQRS.Queries;
using certman.Models;
using certman.Services;
using Dapper;
using MediatR;

namespace certman.CQRS.Commands;

public record CreateCACertCommand(CreateCACertDto Dto): IRequest<CACert>;

public class CreateCACertCommandHandler : CertmanHandler<CreateCACertCommand, CACert>
{
    private readonly IOpenSSL _ssl;
    private readonly IMediator _mediator;

    public CreateCACertCommandHandler(IConfiguration config, IOpenSSL ssl, IMediator mediator) : base(config)
    {
        _ssl = ssl;
        _mediator = mediator;
    }

    protected override async Task<CACert> ExecuteAsync(CreateCACertCommand request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnection();
        
        //return if cert already exists, checking the db by name
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Name = @name", new {name = request.Dto.Name});
        if (cert != null)
        {
            await connection.CloseAsync();
            throw new Exception("CA Cert with that name already exists");
        }

        // create the private key file with the OpenSSL class
        var keyfile = await _ssl.CreatePrivateKey(request.Dto.Name);

        // create the PEM file with the OpenSSL class
        var pemfile = await _ssl.CreatePEMFile(keyfile);

        // insert cert into db
        await using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText =
            "INSERT INTO CACerts (Name, Keyfile, Pemfile, CreatedAt) VALUES (@name, @keyfile, @pemfile, @createdAt); SELECT last_insert_rowid();";
        insertCommand.Parameters.AddWithValue("@name", request.Dto.Name);
        insertCommand.Parameters.AddWithValue("@keyfile", keyfile);
        insertCommand.Parameters.AddWithValue("@pemfile", pemfile);
        insertCommand.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
        var id = await insertCommand.ExecuteScalarAsync(ctoken);

        await connection.CloseAsync();

        // move files from workdir to store
        var keyFile = Path.Combine(Config["Workdir"], keyfile);
        var keyFileDest = Path.Combine(Config["Store"], keyfile);
        File.Move(keyFile, keyFileDest);
            
        var pemFile = Path.Combine(Config["Workdir"], pemfile);
        var pemFileDest = Path.Combine(Config["Store"], pemfile);
        File.Move(pemFile, pemFileDest);

        return (await _mediator.Send(new GetCACertQuery(Convert.ToInt32(id)), ctoken))!;
    }
}