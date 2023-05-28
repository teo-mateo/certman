namespace certman.Services;

public interface IOpenSSL
{
    public Task<string> CreatePrivateKey(string name);
    public Task<string> CreatePEMFile(string keyFile);
    public Task<(string keyFile, string csrFile)> CreateKeyAndCsr(string name, CsrInfo csrInfo);
    public Task<string> CreateExtFile(string name, string[] dnsNames, string[] ipAddresses);

    public Task<string> CreateSignedCert(
        string name, 
        string keyFileCA, 
        string pemFileCA, 
        string csrFile,
        string extFile);

    public Task<string> BundleSelfSignedCert(string name, string keyFile, string crtFile, string password);

}