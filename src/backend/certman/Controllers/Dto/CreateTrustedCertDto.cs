using System.ComponentModel.DataAnnotations;

namespace certman.Controllers.Dto;

public record CreateTrustedCertDto(
    [Required]
    string Name,
    string? Country,
    string? State,
    string? Locality,
    string? Organization,
    string? OrganizationUnit,
    string CommonName,
    string[] DnsNames,
    string[]? IpAddresses,
    [Required]
    string Password
);
