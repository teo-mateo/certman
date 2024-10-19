using certman.Controllers.Dto;
using certman.CQRS.Queries;
using certman.Extensions;
using certman.Models;
using certman.Services;
using Dapper;
using Heapzilla.Common.Filesystem;
using MediatR;
using Microsoft.Data.Sqlite;

namespace certman.CQRS.Commands.CACerts;

public record CreateCACertCommand(CreateCACertDto Dto): IRequest<CACert>;

public class CreateCACertCommandHandler(IConfiguration config, IOpenSSL ssl, IMediator mediator, ILogger<CreateCACertCommandHandler> logger)
    : CertmanHandler<CreateCACertCommand, CACert>(config, logger)
{
    protected override async Task<CACert> ExecuteAsync(CreateCACertCommand request, CancellationToken ctoken)
    {
        logger.LogInformation("Creating CA cert with name {Name}", request.Dto.Name);
        
        await using var connection = await GetOpenConnectionAsync();
        
        //return if cert already exists, checking the db by name
        await ThrowIfCertWithNameAlreadyExists(request, connection);

        // create the private key file with the OpenSSL class
        var keyfile = await ssl.CreatePrivateKey(request.Dto.Name);
        logger.LogInformation("Created private key file {Keyfile}", keyfile);

        // create the PEM file with the OpenSSL class
        var pemfile = await ssl.CreatePEMFile(keyfile);
        logger.LogInformation("Created PEM file {Pemfile}", pemfile);

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
        MoveFromWorkdirToStore(keyfile);
        MoveFromWorkdirToStore(pemfile);

        return (await mediator.Send(new GetCACertQuery(Convert.ToInt32(id)), ctoken))!;
    }

    private void MoveFromWorkdirToStore(string file)
    {
        var source = Path.Combine(_config["Workdir"]!, file).ThrowIfFileNotExists();
        var dest = Path.Combine(_config["Store"]!, file).ThrowIfFileExists();
        File.Move(source, dest);
    }

    private static async Task ThrowIfCertWithNameAlreadyExists(CreateCACertCommand request, SqliteConnection connection)
    {
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Name = @name",
            new { name = request.Dto.Name });
        if (cert != null)
        {
            await connection.CloseAsync();
            throw new Exception("CA Cert with that name already exists");
        }
    }
}