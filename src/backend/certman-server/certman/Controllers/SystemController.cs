using certman.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

//ServerController class deriving from ControllerBase, with one action GET /server/version that returns the string "1.0" not a Delegate
[Route("[controller]")]
public class SystemController : ControllerBase
{
    private readonly IConfiguration _config;
    public SystemController(IConfiguration config)
    {
        _config = config;
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
    public IActionResult GetSystemInfo()
    {
        object info = new
        {
            ConnectionString = _config.GetConnectionString(),
            Workdir = _config["Workdir"],
            Store = _config["Store"],
            Database = _config["Database"],
        };

        return new JsonResult(info);
    }
}
