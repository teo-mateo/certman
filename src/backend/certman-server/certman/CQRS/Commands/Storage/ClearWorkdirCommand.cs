using MediatR;

namespace certman.CQRS.Commands.Storage;

public record ClearWorkdirCommand(): IRequest<Unit>;

public class ClearWorkdirCommandHandler : CertmanHandler<ClearWorkdirCommand, Unit>
{
    public ClearWorkdirCommandHandler(IConfiguration config) : base(config) { }

    protected override Task<Unit> ExecuteAsync(ClearWorkdirCommand request, CancellationToken ctoken)
    {
        foreach (var file in Directory.GetFiles(_config["Workdir"]))
        {
            File.Delete(file);
        }

        return Task.FromResult(Unit.Value);
    }
}