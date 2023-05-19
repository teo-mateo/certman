namespace certman.Services;

public class CsrInfo
{
    public string Country { get; set; } = "";
    public string State { get; set; } = "";
    public string Locality { get; set; } = "";
    public string Organization { get; set; } = "";
    public string OrganizationUnit { get; set; } = "";
    public string CommonName { get; set; } = "";
}