using certman.Extensions;
using certman.Routes;
using certman.Services;

var builder = WebApplication.CreateBuilder(args);

//add swagger
builder.AddSwagger();

// add cors
builder.Services.AddCors();

// add logging
builder.Logging.AddConsole();

// add application services
builder.Services.AddSingleton<IOpenSSL, OpenSSL>();

// add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.Logger.LogInformation("Starting Certman API...");

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/", () => "Hello World!")
    .WithDescription("HelloWorld");

app.MapGet("/server-version", ServerVersion.GetServerVersion)
    .WithDescription("Get the server version");

app.MapPost("/create-db", DbUtils.CreateDb)
    .WithDescription("Create the database");

app.MapControllers();

app.Run();
