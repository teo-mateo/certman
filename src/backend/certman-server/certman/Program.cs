using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text;
using certman.CQRS.Commands.Startup;
using certman.Extensions;
using certman.Services;
using MediatR;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

// Set up OpenTelemetry 
const string SERVICE_NAME = "certman-backend-api";
const string SERVICE_VERSION = "1.0.0";

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: SERVICE_NAME, serviceVersion: SERVICE_VERSION);

string username() => builder.Configuration["OpenTelemetry:ElasticUsername"]!;
string password() => builder.Configuration["OpenTelemetry:ElasticPassword"]!;
string basicAuthHeader() => Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username()}:{password()}"));
string elasticEndpoint() => builder.Configuration["OpenTelemetry:ElasticEndpoint"]!;

var meter = new Meter(SERVICE_NAME);

// add opentelemetry logging
builder.Logging.AddOpenTelemetry(opt =>
{
    opt.IncludeScopes = true;
    opt.ParseStateValues = true;
    opt.IncludeFormattedMessage = true;
    
    opt.SetResourceBuilder(resourceBuilder)
        .AddOtlpExporter(otlpOpt =>
        {
            otlpOpt.Endpoint = new Uri(elasticEndpoint());
            otlpOpt.Headers = $"Authorization=Basic {basicAuthHeader}";
        });
});

// add OpenTelemetry tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(cfgTracing =>
    {
        cfgTracing
            .AddSource(SERVICE_NAME)   //is this necessary?
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation(opt => opt.RecordException = true)
            .AddHttpClientInstrumentation(opt => opt.RecordException = true)
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(elasticEndpoint());
                opt.Headers = $"Authorization=Basic {basicAuthHeader()}";
            });

    })
    .WithMetrics(cfgMetrics =>
    {
        cfgMetrics
            .AddMeter(SERVICE_NAME)
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(elasticEndpoint());
                opt.Headers = $"Authorization=Basic {basicAuthHeader()}";
            });
    });

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
        app.Logger.LogInformation("Webroot: {Webroot}", app.Environment.WebRootPath);
        var mediator = app.Services.GetRequiredService<IMediator>();
        mediator.Send(new EnsureDatabaseExistsCommand()).GetAwaiter().GetResult();
        mediator.Send(new EnsureDataDirsExistCommand()).GetAwaiter().GetResult();
        mediator.Send(new EnsureCertificateExistsCommand()).GetAwaiter().GetResult();
    });

var info = new
{
    Now = DateTime.Now,
    Application = "Certman API",
};
app.Logger.LogInformation("Starting Certman API; info: {Info}", info);

// use swagger
app.UseSwaggerEx();

// enable cors, all origins, all methods, all headers
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.MapControllers();


try
{
    app.Run();
}
catch (Exception ex)
{
    
    Console.WriteLine(ex.Message);
}
