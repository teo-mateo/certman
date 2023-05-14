using certman.Extensions;
using certman.Models;
using certman.Services;
using Dapper;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

//add swagger
builder.AddSwagger();

// add cors
builder.Services.AddCors();

var app = builder.Build();

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/", () => "Hello World!");
app.MapGet("/server-version", () => new
{
    ServerVersion = "1.0"
});

app.MapPost("/create-db", async () =>
{
    //get connection string
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

    //open connection
    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    // execute script from scripts/db.sql
    await using var scriptCommand = connection.CreateCommand();
    scriptCommand.CommandText = await File.ReadAllTextAsync("scripts/db.sql");
    await scriptCommand.ExecuteNonQueryAsync();

    await connection.CloseAsync();
    return "ok";
});


// POST endpoint "/ca-certs" to create a new CA cert; returns the cert's id
app.MapPost("/ca-certs", async (certman.Dtos.CreateCACertDto dto) =>
{
    //get connection string
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

    //open connection
    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();
    
    // create the private key file with the OpenSSL class
    var keyfile = OpenSSL.CreatePrivateKey(dto.Name);
    
    // create the PEM file with the OpenSSL class
    var pemfile = OpenSSL.CreatePEMFile(keyfile);

    // insert cert into db
    await using var insertCommand = connection.CreateCommand();
    insertCommand.CommandText = "INSERT INTO CACerts (Name, Keyfile, Pemfile, CreatedAt) VALUES (@name, @keyfile, @pemfile, @createdAt); SELECT last_insert_rowid();";
    insertCommand.Parameters.AddWithValue("@name", dto.Name);
    insertCommand.Parameters.AddWithValue("@keyfile", keyfile);
    insertCommand.Parameters.AddWithValue("@pemfile", pemfile);
    insertCommand.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
    var id = await insertCommand.ExecuteScalarAsync();

    await connection.CloseAsync();
    return id;
});

// GET endpoint to return all CA Certs. Returns an array of CACert objects
app.MapGet("/ca-certs", async () =>
{
    // get connection string
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
    
    // open connection
    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();
    
    // get all certs from db with dapper
    var certs = await connection.QueryAsync<CACert>("SELECT * FROM CACerts");
    return certs;
});


app.Run();
