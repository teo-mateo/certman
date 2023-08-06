using MediatR;

using Settings = System.Collections.Generic.Dictionary<string, string>;

namespace certman.CQRS.Queries;

public record GetSettingsQuery()  : IRequest<Settings>;

public class GetSettingsQueryHandler : CertmanHandler<GetSettingsQuery, Settings>
{
    public GetSettingsQueryHandler(IConfiguration config, ILogger<GetSettingsQueryHandler> logger) : base(config, logger)
    {
    }

    protected override Task<Settings> ExecuteAsync(GetSettingsQuery request, CancellationToken ctoken)
    {
        var settings = new Settings();
        foreach (var (key, value) in _config.AsEnumerable())
        {
            settings.Add(key, value);
        }
        
        settings.add("IMAGE_VERSION", Environment.GetEnvironmentVariable("IMAGE_VERSION") ?? "Unknown");

        return Task.FromResult(settings);
    }
}