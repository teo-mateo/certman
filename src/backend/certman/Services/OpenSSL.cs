using System.Diagnostics;
using System.Text;
using certman.Extensions;

namespace certman.Services;

public interface IOpenSSL
{
    public Task<string> CreatePrivateKey(string name);
    public Task<string> CreatePEMFile(string keyfile);
    public Task<(string keyfile, string csrfile)> CreateKeyAndCsr(string name, CsrInfo csrInfo);
    public Task<string> CreateExtFile(string name, string[] dnsNames, string[] ipAddresses);

    public Task<string> CreateSelfSignedCert(
        string name, 
        string keyfileCA, 
        string pemfileCA, 
        string csrfile,
        string extfile);

    public Task<string> BundleSelfSignedCert(string name, string keyfile, string crtfile);

}

public class CsrInfo
{
    public string Country { get; set; } = "";
    public string State { get; set; } = "";
    public string Locality { get; set; } = "";
    public string Organization { get; set; } = "";
    public string OrganizationUnit { get; set; } = "";
    public string CommonName { get; set; } = "";
}

public class OpenSSL: IOpenSSL
{
    //ILogger
    private readonly ILogger<OpenSSL> _logger;
    private readonly string _opensslExecutable;
    private readonly string _workdir;
    
    //ctor
    // Thoughts: instead of passing an IConfiguration from where we pick the values, 
    // look into how to use the Options pattern to inject the values
    public OpenSSL(IConfiguration configuration, ILogger<OpenSSL> logger)
    {
        _opensslExecutable = configuration["OpenSSLExecutable"];
        _workdir = configuration["Workdir"];
        _logger = logger;
    }

    /// <summary>
    /// This method creates a private key with the given name and returns key file name.
    /// </summary>
    public async Task<string> CreatePrivateKey(string name)
    {
        var outputKeyFile = Path.Combine(_workdir, $"{name}.key").ThrowIfFileExists();

        await RunCommand($"genrsa -out \"{outputKeyFile}\" 2048");
        
        // return the output file
        return $"{name}.key";
    }

    /// <summary>
    /// this method creates the PEM file for the given private key and returns the PEM file name.
    /// </summary>
    public async Task<string> CreatePEMFile(string keyfile)
    {
        var inputKeyFile = Path.Combine(_workdir, keyfile).ThrowIfFileNotExists();

        // the name of the pem file is the same as the key file, but with a .pem extension
        var name = Path.GetFileNameWithoutExtension(keyfile);
        
        var outputPemFile = Path.Combine(_workdir, $"{name}.pem").ThrowIfFileExists();

        await RunCommand($"req -new -x509 -key \"{inputKeyFile}\" -out \"{outputPemFile}\" -days 3650 -subj /CN={name}");
        
        // return the output file
        return $"{name}.pem";
        
    }
    
    private async Task<string> CreateCnfFile(string name, CsrInfo csrInfo)
    {
        // create the cnf file from the template
        var outputCnfFile = Path.Combine(_workdir, $"{name}.cnf").ThrowIfFileExists();
        var cnf = CnfTemplate
            .Replace("{country}", csrInfo.Country )
            .Replace("{state}", csrInfo.State)
            .Replace("{locality}", csrInfo.Locality)
            .Replace("{organization}", csrInfo.Organization)
            .Replace("{organizationUnit}", csrInfo.OrganizationUnit)
            .Replace("{commonName}", csrInfo.CommonName);

        await using var writer = File.CreateText(outputCnfFile);
        await writer.WriteAsync(cnf);
        return outputCnfFile;
    }
    
    public async Task<(string keyfile, string csrfile)> CreateKeyAndCsr(string name, CsrInfo csrInfo)
    {
        // create the cnf file using csrInfo and the template
        var inputCnfFile = await CreateCnfFile(name, csrInfo);
        var outputKeyFile = Path.Combine(_workdir, $"{name}.key").ThrowIfFileExists();
        var outputCsrFile = Path.Combine(_workdir, $"{name}.csr").ThrowIfFileExists();

        await RunCommand(
            $"req -new -newkey rsa:2048 -nodes -keyout \"{outputKeyFile}\" -out \"{outputCsrFile}\" -config \"{inputCnfFile}\"");

        return (
            keyfile: $"{name}.key", 
            csrfile: $"{name}.csr");
    }

    public async Task<string> CreateExtFile(string name, string[] dnsNames, string[] ipAddresses)
    {
        // if ext file with name exists, throw exception
        var outputExtFile = Path.Combine(_workdir, $"{name}.ext").ThrowIfFileExists();

        var stringBuilder = new StringBuilder(ExtTemplate);
        
        // foreach dnsNames with index
        for (var i = 0; i < dnsNames.Length; i++)
        {
            stringBuilder.AppendLine($"DNS.{i+1} = {dnsNames[i]}");
        }
        
        // foreach ipAddresses with index
        for (var i = 0; i < ipAddresses.Length; i++)
        {
            stringBuilder.AppendLine($"IP.{i+1} = {ipAddresses[i]}");
        }
        
        // create extFile and write sb to extFile
        await File.WriteAllTextAsync(outputExtFile, stringBuilder.ToString());
        return outputExtFile;
    }

    public async Task<string> CreateSelfSignedCert(string name, string keyfileCA, string pemfileCA, string csrfile, string extfile)
    {
        var inputKeyFileCA = Path.Combine(_workdir, keyfileCA).ThrowIfFileNotExists();
        var inputPemFileCA = Path.Combine(_workdir, pemfileCA).ThrowIfFileNotExists();
        var inputCsrFile = Path.Combine(_workdir, csrfile).ThrowIfFileNotExists();
        var inputExtFile = Path.Combine(_workdir, extfile).ThrowIfFileNotExists();
        var outputCrtFile = Path.Combine(_workdir, $"{name}.crt").ThrowIfFileExists();
        await RunCommand($"x509 -req -in \"{inputCsrFile}\" -CA \"{inputPemFileCA}\" -CAkey \"{inputKeyFileCA}\" -CAcreateserial -out \"{outputCrtFile}\" -days 365 -sha256 -extfile \"{inputExtFile}\"");
        return $"{name}.crt";
    }

    public async Task<string> BundleSelfSignedCert(string name, string keyfile, string crtfile)
    {
        var inputKeyFile = Path.Combine(_workdir, keyfile).ThrowIfFileNotExists();
        var inputCrtFile = Path.Combine(_workdir, crtfile).ThrowIfFileNotExists();
        var outputPfxFile = Path.Combine(_workdir, $"{name}.pfx").ThrowIfFileExists();
        await RunCommand($"pkcs12 -export -out \"{outputPfxFile}\" -inkey \"{inputKeyFile}\" -in \"{inputCrtFile}\"");
        return $"{name}.pfx";
    }

    private async Task RunCommand(string arguments)
    {
        _logger.LogInformation("[OPENSSL] {Arguments}", arguments);
        
        // create the openssl command to create a PEM file
        var startInfo = new ProcessStartInfo(_opensslExecutable, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        
        // run the openssl command
        var process = Process.Start(startInfo)!;
        process.OutputDataReceived += (sender, args) => _logger.LogInformation("[OPENSSL] {Output}", args.Data);
        process.ErrorDataReceived += (sender, args) => _logger.LogError("[OPENSSL] {Error}", args.Data);
        await process.WaitForExitAsync();
        process.ThrowIfBadExit();
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
CN = {commonName}   
";

    private const string ExtTemplate = @"authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
";    
    
    
}