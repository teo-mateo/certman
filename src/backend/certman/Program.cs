using System.Reflection;
using certman.Extensions;
using certman.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.Logger.LogInformation("Starting Certman API...");

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

app.Run();
