using System.Diagnostics;

namespace certman.Services;

public interface IOpenSSL
{
    public string CreatePrivateKey(string name);
    public string CreatePEMFile(string keyfile);
    public Task<(string keyfile, string csrfile)> CreateKeyAndCsr(string name, CsrInfo csrInfo);
}

public class CsrInfo
{
    public string Country { get; set; } = "";
    public string State { get; set; } = "";
    public string Locality { get; set; } = "";
    public string Organization { get; set; } = "";
    public string OrganizationUnit { get; set; } = "";
    public string DnsName { get; set; } = "";
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
        
        RunCommand($"genrsa -out \"{outputKeyFile}\" 2048");
        
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

        RunCommand($"req -new -x509 -key \"{inputKeyFile}\" -out \"{outputPemFile}\" -days 3650 -subj /CN={name}");
        
        // return the output file
        return $"{name}.pem";
        
    }

    private const string CnfTemplate = @"[req]
distinguished_name = req_distinguished_name
prompt = no

[req_distinguished_name]
C = {country}
ST = {state}
L = {locality}
O = {organization}
OU = {organizationUnit}
CN = {dnsName}";

    private async Task<string> CreateCnfFile(string name, CsrInfo csrInfo)
    {
        // create the cnf file from the template
        var cnfFile = Path.Combine(_workdir, $"{name}.cnf");
        var cnf = CnfTemplate
            .Replace("{country}", csrInfo.Country )
            .Replace("{state}", csrInfo.State)
            .Replace("{locality}", csrInfo.Locality)
            .Replace("{organization}", csrInfo.Organization)
            .Replace("{organizationUnit}", csrInfo.OrganizationUnit)
            .Replace("{dnsName}", csrInfo.DnsName);

        await using var writer = File.CreateText(cnfFile);
        await writer.WriteAsync(cnf);
        return cnfFile;
    }
    
    public async Task<(string keyfile, string csrfile)> CreateKeyAndCsr(string name, CsrInfo csrInfo)
    {
        // create the cnf file using csrInfo and the template
        var inputCnfFile = await CreateCnfFile(name, csrInfo);
        var outputKeyFile = Path.Combine(_workdir, $"{name}.key");
        var outputCsrFile = Path.Combine(_workdir, $"{name}.csr");

        RunCommand(
            $"req -new -newkey rsa:2048 -nodes -keyout \"{outputKeyFile}\" -out \"{outputCsrFile}\" -config \"{inputCnfFile}\"");

        return (
            keyfile: $"{name}.key", 
            csrfile: $"{name}.csr");
    }

    private void RunCommand(string arguments)
    {
        // create the openssl command to create a PEM file
        var startInfo = new ProcessStartInfo(_opensslExecutable, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        // run the openssl command
        var process = Process.Start(startInfo)!;
        process.WaitForExit();
        process.ThrowIfBadExit();
    }
}