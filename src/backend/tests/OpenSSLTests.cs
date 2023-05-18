using certman.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace tests;

public class OpenSSLTests: IDisposable
{
    private const string WorkDir = @"C:\temp\certman_workdir";
    private const string OpenSSLExecutable = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
    
    private readonly IOpenSSL _openSSL;
    
    public OpenSSLTests()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Workdir"]).Returns(WorkDir);
        configMock.Setup(x => x["OpenSSLExecutable"]).Returns(OpenSSLExecutable);

        _openSSL = new OpenSSL(configMock.Object, NullLogger<OpenSSL>.Instance);
        
        
    }

    [Fact]
    public void PitOfSuccess()
    {
        Assert.True(true);
    }
    
    [Fact]
    public void OpenSSL_CreatePrivateKey_Test()
    {
        _openSSL.CreatePrivateKey("test");
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.key")));
    }

    [Fact]
    public void OpenSSL_CreatePEMFile_Test()
    {
        var keyFile = _openSSL.CreatePrivateKey("test");
        _openSSL.CreatePEMFile(keyFile);
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.pem")));
    }

    [Fact]
    public async Task OpenSSL_CreateKeyAndCsr_Test()
    {
        _openSSL.CreatePrivateKey("test");
        var result = await _openSSL.CreateKeyAndCsr("test", new CsrInfo()
        {
            Country = "BE",
            DnsName = "test.local",
            Locality = "Brussels",
            Organization = "Test",
            OrganizationUnit = "TestUnit",
            State = "BrusselsState"
        });
        
        result.keyfile.Should().Be("test.key");
        result.csrfile.Should().Be("test.csr");
        
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.key")));
        Assert.True(File.Exists(Path.Combine(WorkDir, "test.csr")));
    }
    
    
    
    public void Dispose()
    {
        //remove all files from the workDir
        foreach (var file in Directory.GetFiles(WorkDir))
        {
            File.Delete(file);
        }
    }
}