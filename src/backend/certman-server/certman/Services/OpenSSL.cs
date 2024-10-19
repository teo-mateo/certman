using System.Diagnostics;
using System.Text;
using certman.Extensions;

namespace certman.Services;

public class OpenSSL(IConfiguration configuration, ILogger<OpenSSL> logger) : IOpenSSL
{
    //ILogger
    private readonly string _opensslExecutable = configuration["OpenSSLExecutable"] ?? throw new Exception("OpenSSLExecutable not found in config");
    private readonly string _workdir = configuration["Workdir"] ?? throw new Exception("Workdir not found in config");
    
    /// <summary>
    /// This method creates a private key with the given name and returns key file name.
    /// </summary>
    public async Task<string> CreatePrivateKey(string name)
    {
        name = name.SanitizeFileName();

        var outputKeyFile = Path.Combine(_workdir, $"{name}.key").ThrowIfFileExists();

        await RunOpenSSLCommand("genrsa", "-out", outputKeyFile, "2048");
        
        // return the output file
        return $"{name}.key";
    }

    /// <summary>
    /// this method creates the PEM file for the given private key and returns the PEM file name.
    /// </summary>
    public async Task<string> CreatePEMFile(string keyFile)
    {
        var inputKeyFile = Path.Combine(_workdir, keyFile).ThrowIfFileNotExists();

        // the name of the pem file is the same as the key file, but with a .pem extension
        var name = Path.GetFileNameWithoutExtension(keyFile);
        
        var outputPemFile = Path.Combine(_workdir, $"{name}.pem").ThrowIfFileExists();

        await RunOpenSSLCommand("req", "-new", "-x509", "-key", inputKeyFile, "-out", outputPemFile, "-days", "3650", "-subj", $"/CN=\"{name}\"");
        
        // return the output file
        return $"{name}.pem";
        
    }
    
    private async Task<string> CreateCnfFile(string name, CsrInfo csrInfo)
    {
        name = name.SanitizeFileName();
        
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
    
    public async Task<(string keyFile, string csrFile)> CreateKeyAndCsr(string name, CsrInfo csrInfo)
    {
        name = name.SanitizeFileName();
        
        // create the cnf file using csrInfo and the template
        var inputCnfFile = await CreateCnfFile(name, csrInfo);
        var outputKeyFile = Path.Combine(_workdir, $"{name}.key").ThrowIfFileExists();
        var outputCsrFile = Path.Combine(_workdir, $"{name}.csr").ThrowIfFileExists();

        await RunOpenSSLCommand(
            "req", "-new", "-newkey", "rsa:2048", "-nodes", "-keyout", outputKeyFile, "-out", outputCsrFile, "-config", inputCnfFile);

        return (
            keyFile: $"{name}.key", 
            csrFile: $"{name}.csr");
    }

    public async Task<string> CreateExtFile(string name, string[] dnsNames, string[] ipAddresses)
    {
        name = name.SanitizeFileName();
        
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
        return $"{name}.ext";
    }

    public async Task<string> CreateSignedCert(string name, string keyFileCA, string pemFileCA, string csrFile, string extFile)
    {
        name = name.SanitizeFileName();
        
        var inputKeyFileCA = Path.Combine(_workdir, keyFileCA).ThrowIfFileNotExists();
        var inputPemFileCA = Path.Combine(_workdir, pemFileCA).ThrowIfFileNotExists();
        var inputCsrFile = Path.Combine(_workdir, csrFile).ThrowIfFileNotExists();
        var inputExtFile = Path.Combine(_workdir, extFile).ThrowIfFileNotExists();
        var outputCrtFile = Path.Combine(_workdir, $"{name}.crt").ThrowIfFileExists();
        await RunOpenSSLCommand(
            "x509", "-req", "-in", inputCsrFile, "-CA", inputPemFileCA, "-CAkey", inputKeyFileCA, "-CAcreateserial", "-out", outputCrtFile, "-days", "365", "-sha256", "-extfile", inputExtFile);
        return $"{name}.crt";
    }

    public async Task<string> BundleSelfSignedCert(string name, string keyFile, string crtFile, string password)
    {
        name = name.SanitizeFileName();
        
        var inputKeyFile = Path.Combine(_workdir, keyFile).ThrowIfFileNotExists();
        var inputCrtFile = Path.Combine(_workdir, crtFile).ThrowIfFileNotExists();
        var outputPfxFile = Path.Combine(_workdir, $"{name}.pfx").ThrowIfFileExists();
        await RunOpenSSLCommand(
            "pkcs12", "-export", "-out", outputPfxFile, "-inkey", inputKeyFile, "-in", inputCrtFile, "-passout", $"pass:{password}");
        return $"{name}.pfx";
    }

    [Obsolete]
    private async Task RunOpenSSLCommand(string arguments)
    {
        var startInfo = CreateStartInfo(arguments);
        await RunOpenSSLCommand(startInfo);
    }

    private async Task RunOpenSSLCommand(params string[] arguments)
    {
        var startInfo = CreateStartInfo(arguments);
        await RunOpenSSLCommand(startInfo);
    }

    private async Task RunOpenSSLCommand(ProcessStartInfo startInfo)
    {
        logger.LogInformation("[OPENSSL] {Arguments}", startInfo.ArgumentList.Aggregate((a, b) => $"{a} {b}"));
        
        // run the openssl command
        var process = new Process()
        {
            StartInfo = startInfo
        };
        
        process.Start();
        
        string output = await process.StandardOutput.ReadToEndAsync();
        if (!string.IsNullOrWhiteSpace(output))
            logger.LogInformation("[OPENSSL] {Output}", output);
        
        string error = await process.StandardError.ReadToEndAsync();
        if (!string.IsNullOrWhiteSpace(error))
            logger.LogError("[OPENSSL] {Error}", error);
        
        await process.WaitForExitAsync();
        process.ThrowIfBadExit(error);        
    }

    private ProcessStartInfo CreateStartInfo(string arguments)
    {
        return new ProcessStartInfo(_opensslExecutable, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
    }

    private ProcessStartInfo CreateStartInfo(IEnumerable<string> arguments)
    {
        var processStartInfo = new ProcessStartInfo(_opensslExecutable)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            processStartInfo.ArgumentList.Add(argument);
        }

        return processStartInfo;
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