using certman.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace tests;

public class OpenSSLTests
{
    private const string WorkDir = @"C:\temp\certman_workdir";
    private const string OpenSSLExecutable = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
    
    private readonly IOpenSSL _openSSL;
    
    public OpenSSLTests()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Workdir"]).Returns(WorkDir);
        configMock.Setup(x => x["OpenSSLExecutable"]).Returns(OpenSSLExecutable);

        var loggerFactory = LoggerFactory.Create(
            builder => builder.AddSimpleConsole(options => options.SingleLine = true));
        var logger = loggerFactory.CreateLogger<OpenSSL>();

        _openSSL = new OpenSSL(configMock.Object, logger);
        
        CleanupWorkDir();
    }

    [Fact]
    public void PleaseFallInThePitOfSuccess()
    {
        Assert.True(true);
    }
    
    [Fact]
    public async Task OpenSSL_CreatePrivateKey_Test()
    {
        await _openSSL.CreatePrivateKey("test");
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.key")));
    }

    [Fact]
    public async Task OpenSSL_CreatePEMFile_Test()
    {
        var keyFile = await _openSSL.CreatePrivateKey("test");
        await _openSSL.CreatePEMFile(keyFile);
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.pem")));
    }

    [Fact]
    public async Task OpenSSL_CreateKeyAndCsr_Test()
    {
        await _openSSL.CreatePrivateKey("test");
        var result = await _openSSL.CreateKeyAndCsr("test", new CsrInfo()
        {
            Country = "BE",
            Locality = "Brussels",
            Organization = "Test",
            OrganizationUnit = "TestUnit",
            State = "BrusselsState",
            CommonName = "CommonName"
        });
        
        result.keyfile.Should().Be("test.key");
        result.csrfile.Should().Be("test.csr");
        
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.key")));
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.csr")));
    }
    
    [Fact]
    public async Task OpenSSL_CreateExtFile_Test()
    {
        var result = await _openSSL.CreateExtFile(
            "test", 
            new[] { "test1.com", "test2.com" },
            new[] { "192.168.1.1", "192.168.1.2" });
        
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.ext")));
        
        var extFile = await File.ReadAllTextAsync(Path.Combine(WorkDir, "test.ext"));
        extFile.Trim().Should().Be(@"authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = test1.com
DNS.2 = test2.com
IP.1 = 192.168.1.1
IP.2 = 192.168.1.2".Trim());
    }

    [Fact]
    public async Task OpenSSL_CreateSelfSignedCert_Test()
    {
        await _openSSL.CreatePrivateKey("test");
        await _openSSL.CreatePEMFile("test.key");
        await _openSSL.CreateKeyAndCsr("mywebsite", new CsrInfo()
        {
            Country = "BE",
            Locality = "Brussels",
            Organization = "Test",
            OrganizationUnit = "TestUnit",
            State = "BrusselsState",
            CommonName = "CommonName"
        });
        await _openSSL.CreateExtFile(
            "mywebsite",
            new[] { "test1.com", "test2.com" },
            new[] { "192.168.1.1" });
        var pfxFile = await _openSSL.CreateSelfSignedCert(
            "mywebsite", 
            "test.key", 
            "test.pem", 
            "mywebsite.csr", 
            "mywebsite.ext");
        
        // assert pfx file exists in workdir
        Assert.True(File.Exists(Path.Combine(WorkDir, pfxFile)));
    }

    private void CleanupWorkDir()
    {
        //remove all files from the workDir
        foreach (var file in Directory.GetFiles(WorkDir))
        {
            File.Delete(file);
        }
    }
}