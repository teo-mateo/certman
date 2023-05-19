namespace certman.Models;

public class Cert
{
    public int Id { get; set; }
    public int CaCertId { get; set; }
    public string Name { get; set; } = "";
    public string AltNames { get; set; } = "";
    public string Keyfile { get; set; } = "";
    public string Csrfile { get; set; } = "";
    public string Extfile { get; set; } = "";
    public string Pfxfile { get; set; } = "";
    public string Password { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
