using certman.CQRS.Commands;
using certman.CQRS.Commands.Storage;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

[Route("[controller]")]
public class UtilitiesController : CertmanController
{
    private readonly IMediator _mediator;

    public UtilitiesController(IConfiguration config, IMediator mediator) : base(config)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Creates the database tables
    /// </summary>
    [HttpPost("create-db-tables")]
    public async Task<ActionResult> CreateDbTables()
    {
        await _mediator.Send(new CreateDbTablesCommand());
        
        return Ok(new {success = true});
    }
}