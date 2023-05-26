namespace certman.Extensions;

public static class WebApplicationExtensions
{
    // extension to use swagger
    public static void UseSwaggerEx(this WebApplication app)
    {
        // use swagger
        app.UseSwagger();

        // use swagger ui
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Certman API v1");
            c.RoutePrefix = string.Empty;
        });
    }
}