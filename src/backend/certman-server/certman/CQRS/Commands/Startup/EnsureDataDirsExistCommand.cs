using certman.Extensions;
using MediatR;

namespace certman.CQRS.Commands.Startup;

public record EnsureDataDirsExistCommand : IRequest<Unit>;

public class EnsureDataDirsExistHandler(IConfiguration config, ILogger<EnsureDataDirsExistHandler> logger)
    : CertmanHandler<EnsureDataDirsExistCommand, Unit>(config, logger)
{
    protected override Task<Unit> ExecuteAsync(EnsureDataDirsExistCommand request, CancellationToken ctoken)
    {
        var workdir = _config.GetWorkdir();
        if (!Directory.Exists(workdir))
        {
            logger.LogInformation("Workdir does not exist, creating {Workdir}...", workdir);
            Directory.CreateDirectory(workdir);
        }
        
        var store = _config["Store"]!;
        if (!Directory.Exists(store))
        {
            logger.LogInformation("Store does not exist, creating {Store}...", store);
            Directory.CreateDirectory(store);
        }
        
        return Task.FromResult(Unit.Value);
    }
}


