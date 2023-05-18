namespace certman.Controllers.Dto;

public record CreateTrustedCertDto(
    string Name,
    string? Country,
    string? State,
    string? Locality,
    string? Organization,
    string? OrganizationUnit,
    string? CommonName,
    string[] DnsNames,
    string[] IpAddresses
);
