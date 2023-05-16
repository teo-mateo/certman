using certman.Services;
using Microsoft.AspNetCore.Mvc;

namespace certman.Routes;

public static partial class Certs
{
    // ReSharper disable once ClassNeverInstantiated.Local
    private record CreateTrustedCertDto(
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
    
    public static readonly Delegate CreateTrustedCert = async (
        [FromServices] IConfiguration config,
        [FromServices] IOpenSSL ssl,
        [FromBody] CreateTrustedCertDto dto) =>
    {
        // get CA Cert info using the dto id
        var caCert = await GetCaCert(config, dto.CACertId);

        await ssl.CreateKeyAndCsr(dto.Name, new CsrInfo()
        {
            Country = dto.Country ?? "",
            State = dto.State ?? "",
            Locality = dto.Locality ?? "",
            Organization = dto.Organization ?? "",
            OrganizationUnit = dto.OrganizationUnit ?? "",
            DnsName = dto.DnsName
        });

    };
}