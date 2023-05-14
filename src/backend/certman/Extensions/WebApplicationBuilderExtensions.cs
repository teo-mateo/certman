using Microsoft.OpenApi.Models;

namespace certman.Extensions;

public static class WebApplicationBuilderExtensions
{
    // extension to add swagger to builder
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        // add swagger doc to builder
        builder.Services.AddEndpointsApiExplorer();

        // add swagger gen to builder
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Certman API", Version = "v1" });
        });

        return builder;
    }
}