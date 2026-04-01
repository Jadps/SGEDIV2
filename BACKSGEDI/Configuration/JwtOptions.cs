using System.ComponentModel.DataAnnotations;

namespace BACKSGEDI.Configuration;

public record JwtOptions
{
    public const string SectionName = "Jwt"; 

    [Required(ErrorMessage = "La clave secreta de JWT es obligatoria para la seguridad.")]
    [MinLength(32, ErrorMessage = "La clave JWT debe tener al menos 32 caracteres.")]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationInMinutes { get; init; } = 60;
}
