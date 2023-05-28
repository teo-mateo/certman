using System.Reflection;
using certman.CQRS.Commands.Startup;
using certman.Extensions;
using certman.Services;
using MediatR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

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
        var mediator = app.Services.GetRequiredService<IMediator>();
        mediator.Send(new EnsureDatabaseExistsCommand()).GetAwaiter().GetResult();
        mediator.Send(new EnsureDataDirsExistCommand()).GetAwaiter().GetResult();
        mediator.Send(new EnsureCertificateExistsCommand()).GetAwaiter().GetResult();
    });

app.Logger.LogInformation("Starting Certman API...");

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
