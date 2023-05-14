namespace certman.Models;

public class Cert
{
    public int Id { get; set; }
    public int CaCertId { get; set; }
    public string Name { get; set; } = "";
    public string? Dns1 { get; set; }
    public string? Dns2 { get; set; }
    public string? Dns3 { get; set; }
    public string Keyfile { get; set; } = "";
    public string Csrfile { get; set; } = "";
    public string Extfile { get; set; } = "";
    public string Pfxfile { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
