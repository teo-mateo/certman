using Swashbuckle.AspNetCore.Annotations;

namespace certman.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder WithDescription(this RouteHandlerBuilder builder, string description)
    {
        builder.WithMetadata(new SwaggerOperationAttribute(description));
        return builder;
    }
}