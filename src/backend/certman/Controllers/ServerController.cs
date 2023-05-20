using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

//ServerController class deriving from ControllerBase, with one action GET /server/version that returns the string "1.0" not a Delegate
[Route("[controller]")]
public class ServerController : ControllerBase
{
    [HttpGet("version")]
    public IActionResult GetServerVersion()
    {
        return new JsonResult( 
            new
            {
                version= "1.0"
            });
    }
}
