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
        deleteCommand.CommandText = "DROP TABLE IF EXISTS CACerts; DROP TABLE IF EXISTS Certs;";
        await deleteCommand.ExecuteNonQueryAsync(ctoken);
        
        // execute script from scripts/db.sql
        await using var scriptCommand = connection.CreateCommand();
        scriptCommand.CommandText = await System.IO.File.ReadAllTextAsync("scripts/db.sql");
        await scriptCommand.ExecuteNonQueryAsync(ctoken);

        await connection.CloseAsync();
        return Unit.Value;
    }
}