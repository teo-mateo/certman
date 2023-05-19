using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

[Route("[controller]")]
public class UtilitiesController : CertmanController
{
    public UtilitiesController(IConfiguration config) : base(config) { }
    
    /// <summary>
    /// Creates the database tables
    /// </summary>
    [HttpPost("create-db-tables")]
    public async Task<ActionResult> CreateDb()
    {
        var connection = await base.GetOpenConnection();
        
        // delete the db if it exists
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DROP TABLE IF EXISTS CACerts; DROP TABLE IF EXISTS Certs;";
        await deleteCommand.ExecuteNonQueryAsync();
        
        // execute script from scripts/db.sql
        await using var scriptCommand = connection.CreateCommand();
        scriptCommand.CommandText = await System.IO.File.ReadAllTextAsync("scripts/db.sql");
        await scriptCommand.ExecuteNonQueryAsync();

        await connection.CloseAsync();
        return Ok(new {success = true});
    }
}