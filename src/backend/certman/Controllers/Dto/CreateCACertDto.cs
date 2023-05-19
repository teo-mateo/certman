using System.ComponentModel.DataAnnotations;

namespace certman.Controllers.Dto;

public record CreateCACertDto(
    [Required]
    string Name
);