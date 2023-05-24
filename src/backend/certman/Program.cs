using System.Reflection;
using certman.CQRS.Commands.Storage;
using certman.Extensions;
using certman.Services;
using MediatR;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// create database if it doesn't exist

//add swagger
builder.AddSwagger();

// add cors
builder.Services.AddCors();

// add logging
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
});

// add application services
builder.Services.AddSingleton<IOpenSSL, OpenSSL>();

// add controllers
builder.Services.AddControllers();

// configure route options
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// add mediatr
builder.Services.AddMediatR(config =>
{
    config.Lifetime = ServiceLifetime.Singleton;
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

var app = builder.Build();

app.Services.GetRequiredService<IHostApplicationLifetime>()
    .ApplicationStarted
    .Register(() =>
    {
        var config = app.Services.GetRequiredService<IConfiguration>();
        var mediator = app.Services.GetRequiredService<IMediator>();

        var connectionString = $"Data Source={config["Database"]}";
        var databaseFile = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (File.Exists(databaseFile))
        {
            app.Logger.LogInformation("Database exists: {DatabaseFile}", databaseFile);
            return;
        }
        
        app.Logger.LogInformation("Database does not exist, creating...");
        mediator.Send(new RecreateDbTablesCommand()).GetAwaiter().GetResult();
    });

app.Logger.LogInformation("Starting Certman API...");

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

app.Run();
