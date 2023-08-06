using certman.CQRS.Queries;
using certman.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

//ServerController class deriving from ControllerBase, with one action GET /server/version that returns the string "1.0" not a Delegate
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IMediator _mediator;
    public SystemController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("version")]
    public IActionResult GetServerVersion()
    {
        return new JsonResult( 
            new
            {
                version= "1.0"
            });
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetSystemInfo()
    {
        var settings = await _mediator.Send(new GetSettingsQuery());

        // only keep following keys
        var keys = new[]
        {
            "Database",
            "Store",
            "Workdir",
            "WEBROOT",
            "OpenSSLExecutable",
            "ENVIRONMENT",
            "applicationName",
            "AllowedHosts",
            "IMAGE_VERSION"
        };
        
        var settings2 = settings.Where(x => keys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        return new JsonResult(settings2);
    }
    
    [HttpGet("all-settings")]
    public async Task<IActionResult> GetAllSettings()
    {
        var settings = await _mediator.Send(new GetSettingsQuery());
        return new JsonResult(settings);
    }
}
