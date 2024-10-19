using MediatR;

using Settings = System.Collections.Generic.Dictionary<string, string>;

namespace certman.CQRS.Queries;

public record GetSettingsQuery  : IRequest<Settings>;

public class GetSettingsQueryHandler(IConfiguration config, ILogger<GetSettingsQueryHandler> logger)
    : CertmanHandler<GetSettingsQuery, Settings>(config, logger)
{
    protected override Task<Settings> ExecuteAsync(GetSettingsQuery request, CancellationToken ctoken)
    {
        var settings = new Settings();
        foreach (var (key, value) in _config.AsEnumerable())
        {
            settings.Add(key, value!);
        }

        return Task.FromResult(settings);
    }
}