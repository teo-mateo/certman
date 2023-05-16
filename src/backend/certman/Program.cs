using System.Runtime.InteropServices;
using certman.Extensions;
using certman.Models;
using certman.Routes;
using certman.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

//add swagger
builder.AddSwagger();

// add cors
builder.Services.AddCors();

// add logging
builder.Logging.AddConsole();

// add application services
builder.Services.AddSingleton<IOpenSSL, OpenSSL>();

var app = builder.Build();

app.Logger.LogInformation("Starting Certman API...");

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/", () => "Hello World!");
app.MapGet("/server-version", ServerVersion.Get);
app.MapPost("/create-db", DbUtils.Post);

// POST endpoint "/ca-certs" to create a new CA cert; returns the certificate's id
app.MapPost("/ca-certs", Certs.CreateCACert);

// GET endpoint to return all CA Certs. Returns an array of CACert objects
app.MapGet("/ca-certs", Certs.GetAllCACerts);

app.Run();
