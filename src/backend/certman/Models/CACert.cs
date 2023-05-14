namespace certman.Models;

// ReSharper disable once InconsistentNaming
public class CACert
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Keyfile { get; set; } = "";
    public string Pemfile { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
