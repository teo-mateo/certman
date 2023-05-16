using System.Diagnostics.CodeAnalysis;
using certman.Models;
using certman.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace certman.Routes;

public static partial class Certs
{
    private record CreateCACertDto(
        string Name
    );
    
    public static readonly Delegate CreateCACert = async (
        [FromServices] IOpenSSL ssl,
        [FromServices] IConfiguration config,
        [FromBody] CreateCACertDto dto) =>
    {
        //get connection string
        var connectionString = config.GetConnectionString("DefaultConnection");

        //open connection
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        // create the private key file with the OpenSSL class
        var keyfile = ssl.CreatePrivateKey(dto.Name);

        // create the PEM file with the OpenSSL class
        var pemfile = ssl.CreatePEMFile(keyfile);

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
        return id;
    };

    public static readonly Delegate GetAllCACerts = async (
        [FromServices] IConfiguration config) => await GetAllCaCerts(config);

    private static async Task<IEnumerable<CACert>> GetAllCaCerts(IConfiguration config)
    {
        // get connection string
        var connectionString = config.GetConnectionString("DefaultConnection");

        // open connection
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        // get all certs from db with dapper
        var certs = await connection.QueryAsync<CACert>("SELECT * FROM CACerts");
        return certs!;
    }

    private static async Task<CACert> GetCaCert(IConfiguration config, int id)
    {
        // get the CA cert from the db
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var cert = await connection.QueryFirstOrDefaultAsync<CACert>("SELECT * FROM CACerts WHERE Id = @id", new {id});
        await connection.CloseAsync();
        return cert;
    }
}