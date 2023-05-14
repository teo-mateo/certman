using System.Diagnostics;

namespace certman.Services;

public class OpenSSL
{
    /// <summary>
    /// This method creates a private key with the given name and returns key file name.
    /// </summary>
    public static string CreatePrivateKey(string name)
    {
        // create the openssl command to create a private key
        var opensslCommand = new ProcessStartInfo("openssl", $"genrsa -out {name}.key 2048")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        // run the openssl command
        var opensslProcess = Process.Start(opensslCommand);
        opensslProcess?.WaitForExit();
        
        // return the output file
        return $"{name}.key";
        
    }

    /// <summary>
    /// this method creates the PEM file for the given private key and returns the PEM file name.
    /// </summary>
    public static string CreatePEMFile(string keyfile)
    {
        // the name of the pem file is the same as the key file, but with a .pem extension
        var name = Path.GetFileNameWithoutExtension(keyfile);

        // create the openssl command to create a PEM file
        var opensslCommand = new ProcessStartInfo("openssl", $"req -new -x509 -key {keyfile} -out {name}.pem -days 3650 -subj /CN={name}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        // run the openssl command
        var opensslProcess = Process.Start(opensslCommand);
        opensslProcess?.WaitForExit();
        
        // return the output file
        return $"{name}.pem";
        
    }
}