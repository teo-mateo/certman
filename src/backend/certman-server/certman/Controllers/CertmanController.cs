using Microsoft.AspNetCore.Mvc;

namespace certman.Controllers;

public class CertmanController : ControllerBase
{
    protected readonly IConfiguration Config;

    public CertmanController(IConfiguration config)
    {
        Config = config;
    }
}