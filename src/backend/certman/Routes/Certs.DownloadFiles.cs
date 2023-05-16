using Microsoft.AspNetCore.Mvc;

namespace certman.Routes;

public static partial class Certs
{
    public static readonly Delegate DownloadKeyFile = async (
        [FromServices] IConfiguration config,
        [FromRoute] int id) =>
    {
        var workdir = config.GetValue<string>("Workdir");
        
        var cert = await GetCaCert(config, id);
        var path = Path.Combine(workdir, cert.Keyfile);
        if (!File.Exists(path))
            throw new FileNotFoundException(path);
        
        // download file
        return Results.File(path, "application/octet-stream", cert.Keyfile);
    };
    
    public static readonly Delegate DownloadPemFile = async (
        [FromServices] IConfiguration config,
        [FromRoute] int id) =>
    {
        var workdir = config.GetValue<string>("Workdir");
        
        var cert = await GetCaCert(config, id);
        var path = Path.Combine(workdir, cert.Pemfile);
        if (!File.Exists(path))
            throw new FileNotFoundException(path);
        
        // download file
        return Results.File(path, "application/octet-stream", cert.Keyfile);
    };
}