using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace certman.Controllers;

public class CertmanController : ControllerBase
{
    protected readonly IConfiguration Config;

    public CertmanController(IConfiguration config)
    {
        Config = config;
    }
}