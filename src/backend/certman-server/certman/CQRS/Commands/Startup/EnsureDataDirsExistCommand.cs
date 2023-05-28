using certman.Extensions;
using MediatR;

namespace certman.CQRS.Commands.Startup;

public record EnsureDataDirsExistCommand : IRequest<Unit>;

public class EnsureDataDirsExistHandler : CertmanHandler<EnsureDataDirsExistCommand, Unit>
{
    public EnsureDataDirsExistHandler(IConfiguration config, ILogger<EnsureDataDirsExistHandler> logger) 
        : base(config, logger) { }

    protected override Task<Unit> ExecuteAsync(EnsureDataDirsExistCommand request, CancellationToken ctoken)
    {
        var workdir = _config.GetWorkdir();
        if (!Directory.Exists(workdir))
        {
            _logger?.LogInformation("Workdir does not exist, creating {Workdir}...", workdir);
            Directory.CreateDirectory(workdir);
        }
        
        var store = _config["Store"];
        if (!Directory.Exists(store))
        {
            _logger?.LogInformation("Store does not exist, creating {Store}...", store);
            Directory.CreateDirectory(store);
        }
        
        return Task.FromResult(Unit.Value);
    }
}


