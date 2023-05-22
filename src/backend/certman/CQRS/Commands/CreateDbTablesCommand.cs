using MediatR;

namespace certman.CQRS.Commands;

public record CreateDbTablesCommand : IRequest<Unit>;

public class CreateDbTablesCommandHandler : CertmanHandler<CreateDbTablesCommand, Unit>
{
    public CreateDbTablesCommandHandler(IConfiguration config) : base(config) { }

    protected override async Task<Unit> ExecuteAsync(CreateDbTablesCommand request, CancellationToken ctoken)
    {
        await using var connection = await GetOpenConnection();
        // delete the db if it exists
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DROP TABLE IF EXISTS Certs; DROP TABLE IF EXISTS CACerts;";
        await deleteCommand.ExecuteNonQueryAsync(ctoken);
        
        // execute script from scripts/db.sql
        await using var scriptCommand = connection.CreateCommand();
        scriptCommand.CommandText = await System.IO.File.ReadAllTextAsync("Scripts/db.sql", ctoken);
        await scriptCommand.ExecuteNonQueryAsync(ctoken);
        
        return Unit.Value;
    }
}