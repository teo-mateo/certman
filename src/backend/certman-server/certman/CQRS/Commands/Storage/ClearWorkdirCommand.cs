using MediatR;

namespace certman.CQRS.Commands.Storage;

public record ClearWorkdirCommand: IRequest<Unit>;

public class ClearWorkdirCommandHandler(IConfiguration config, ILogger<ClearWorkdirCommandHandler> logger) : CertmanHandler<ClearWorkdirCommand, Unit>(config, logger)
{
    protected override Task<Unit> ExecuteAsync(ClearWorkdirCommand request, CancellationToken ctoken)
    {
        foreach (var file in Directory.GetFiles(_config["Workdir"]!))
        {
            File.Delete(file);
        }

        return Task.FromResult(Unit.Value);
    }
}