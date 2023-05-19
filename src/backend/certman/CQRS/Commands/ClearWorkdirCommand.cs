using MediatR;

namespace certman.CQRS.Commands;

public record ClearWorkdirCommand(): IRequest<Unit>;

public class ClearWorkdirCommandHandler : CertmanHandler<ClearWorkdirCommand, Unit>
{
    public ClearWorkdirCommandHandler(IConfiguration config) : base(config) { }

    protected override Task<Unit> ExecuteAsync(ClearWorkdirCommand request, CancellationToken ctoken)
    {
        foreach (var file in Directory.GetFiles(Config["Workdir"]))
        {
            File.Delete(file);
        }

        return Task.FromResult(Unit.Value);
    }
}