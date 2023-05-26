using MediatR;

namespace certman.CQRS.Commands.Startup;

public record EnsureDataDirsExistCommand : IRequest<Unit>;

public class EnsureDataDirsExistHandler : CertmanHandler<EnsureDataDirsExistCommand, Unit>
{
    private readonly ILogger _logger;

    public EnsureDataDirsExistHandler(IConfiguration config, ILogger logger) : base(config)
    {
        _logger = logger;
    }

    protected override Task<Unit> ExecuteAsync(EnsureDataDirsExistCommand request, CancellationToken ctoken)
    {
        var workdir = Config["Workdir"];
        if (!Directory.Exists(workdir))
        {
            _logger.LogInformation("Workdir does not exist, creating {Workdir}...", workdir);
            Directory.CreateDirectory(workdir);
        }
        
        var store = Config["Store"];
        if (!Directory.Exists(store))
        {
            _logger.LogInformation("Store does not exist, creating {Store}...", store);
            Directory.CreateDirectory(store);
        }
        
        return Task.FromResult(Unit.Value);
    }
}


