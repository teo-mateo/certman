namespace certman.Controllers.Dto;

public record CreateTrustedCertDto(
    int CACertId,
    string Name,
    //string? CommonName,
    string? Country,
    string? State,
    string? Locality,
    string? Organization,
    string? OrganizationUnit,
    string DnsName
    //string? IpAddress,
    //string? EmailAddress
);
