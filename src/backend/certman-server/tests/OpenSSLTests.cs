using certman.Extensions;
using certman.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    
    // run test with multiple data sets
    [Theory]
    [InlineData("test")]
    [InlineData("this is a test")]
    public async Task OpenSSL_CreatePrivateKey_Test(string name)
    {
        await _openSSL.CreatePrivateKey(name);
        Assert.True(File.Exists(Path.Combine(WorkDir, $"{name.SanitizeFileName()}.key")));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("this is a test")]
    public async Task OpenSSL_CreatePEMFile_Test(string name)
    {
        var keyFile = await _openSSL.CreatePrivateKey(name);
        await _openSSL.CreatePEMFile(keyFile);
        Assert.True(File.Exists(Path.Combine(WorkDir, $"{name.SanitizeFileName()}.pem")));
    }

    [Theory]
    [InlineData("mywebsite")]
    [InlineData("my website")]
    public async Task OpenSSL_CreateKeyAndCsr_Test(string name)
    {
        var result = await _openSSL.CreateKeyAndCsr(name, new CsrInfo()
        {
            Country = "BE",
            Locality = "Brussels",
            Organization = "Test",
            OrganizationUnit = "TestUnit",
            State = "BrusselsState",
            CommonName = "CommonName"
        });
        
        result.keyFile.Should().Be($"{name.SanitizeFileName()}.key");
        result.csrFile.Should().Be($"{name.SanitizeFileName()}.csr");
        
        Assert.True(File.Exists(Path.Combine(WorkDir, $"{name.SanitizeFileName()}.key")));
        Assert.True(File.Exists(Path.Combine(WorkDir, $"{name.SanitizeFileName()}.csr")));
    }
    
    [Theory]
    [InlineData("test")]
    [InlineData("this is a test")]
    public async Task OpenSSL_CreateExtFile_Test(string name)
    {
        var result = await _openSSL.CreateExtFile(name, 
            new[] { "test1.com", "test2.com" },
            new[] { "192.168.1.1", "192.168.1.2" });
        
        Assert.True(File.Exists(Path.Combine(WorkDir, $"{name.SanitizeFileName()}.ext")));
        
        var extFile = await File.ReadAllTextAsync(Path.Combine(WorkDir, $"{name.SanitizeFileName()}.ext"));
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

    [Theory]
    [InlineData("authorityame", "mywebsite")]
    [InlineData("authority name", "my cool web site")]
    public async Task OpenSSL_CreateSelfSignedCert_Test(string authorityName, string name)
    {
        var keyFileCA = await _openSSL.CreatePrivateKey(authorityName);
        var pemFileCA = await _openSSL.CreatePEMFile(keyFileCA);
        var (keyFile, csrFile) = await _openSSL.CreateKeyAndCsr(
            name,
            new CsrInfo()
            {
                Country = "BE",
                Locality = "Brussels",
                Organization = "Test",
                OrganizationUnit = "TestUnit",
                State = "BrusselsState",
                CommonName = "CommonName"
            });
        var extFile = await _openSSL.CreateExtFile(
            name,
            new[] { "test1.com", "test2.com" },
            new[] { "192.168.1.1" });
        var crtFile = await _openSSL.CreateSignedCert(
            name, 
            keyFileCA, 
            pemFileCA, 
            csrFile, 
            extFile);
        
        // assert crt file exists in workdir
        Assert.True(File.Exists(Path.Combine(WorkDir, crtFile)));
    }
    
    //unit test to test the bundle self signed cert
    [Theory]
    [InlineData("authorityame", "mywebsite")]
    [InlineData("authority name", "my cool web site")]
    public async Task OpenSSL_BundleSelfSignedCert_Test(string authorityName, string name)
    {
        var keyFileCA = await _openSSL.CreatePrivateKey(authorityName);
        var pemFileCA = await _openSSL.CreatePEMFile(keyFileCA);
        var (keyFile, csrFile) = await _openSSL.CreateKeyAndCsr(
            name,
            new CsrInfo()
            {
                Country = "BE",
                Locality = "Brussels",
                Organization = "Test",
                OrganizationUnit = "TestUnit",
                State = "BrusselsState",
                CommonName = "CommonName"
            });
        var extFile = await _openSSL.CreateExtFile(
            name,
            new[] { "test1.com", "test2.com" },
            new[] { "192.168.1.1" });
        var crtFile = await _openSSL.CreateSignedCert(
            name, 
            keyFileCA, 
            pemFileCA, 
            csrFile, 
            extFile);
        
        var pfxFile = await _openSSL.BundleSelfSignedCert(name, keyFile, crtFile, "test-password");
        
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