using System.Diagnostics;

namespace certman.Services;

public interface IOpenSSL
{
    public string CreatePrivateKey(string name);
    public string CreatePEMFile(string keyfile);
}

public class OpenSSL: IOpenSSL
{
    //ILogger
    private readonly ILogger<OpenSSL> _logger;
    private readonly string _opensslExecutable;
    private readonly string _workdir;
    
    //ctor
    public OpenSSL(IConfiguration configuration, ILogger<OpenSSL> logger)
    {
        _opensslExecutable = configuration["OpenSSLExecutable"];
        _workdir = configuration["Workdir"];
        _logger = logger;
    }

    /// <summary>
    /// This method creates a private key with the given name and returns key file name.
    /// </summary>
    public string CreatePrivateKey(string name)
    {
        var outputKeyFile = Path.Combine(_workdir, $"{name}.key");
        
        if (File.Exists(outputKeyFile))
            throw new Exception($"Key file {name}.key already exists in the work dir.");
        
        // create the openssl command to create a private key
        var startInfo = new ProcessStartInfo(_opensslExecutable, $"genrsa -out \"{outputKeyFile}\" 2048")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        // run the openssl command
        var process = Process.Start(startInfo)!;
        process.WaitForExit();
        process.ThrowIfBadExit();
        
        // return the output file
        return $"{name}.key";
        
    }

    /// <summary>
    /// this method creates the PEM file for the given private key and returns the PEM file name.
    /// </summary>
    public string CreatePEMFile(string keyfile)
    {
        var inputKeyFile = Path.Combine(_workdir, keyfile);
        if (!File.Exists(inputKeyFile))
            throw new FileNotFoundException("Key file not found.", inputKeyFile);
        
        // the name of the pem file is the same as the key file, but with a .pem extension
        var name = Path.GetFileNameWithoutExtension(keyfile);
        
        var outputPemFile = Path.Combine(_workdir, $"{name}.pem");

        // create the openssl command to create a PEM file
        var startInfo = new ProcessStartInfo(_opensslExecutable, $"req -new -x509 -key \"{inputKeyFile}\" -out \"{outputPemFile}\" -days 3650 -subj /CN={name}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        // run the openssl command
        var process = Process.Start(startInfo)!;
        process.WaitForExit();
        process.ThrowIfBadExit();
        
        // return the output file
        return $"{name}.pem";
        
    }
}