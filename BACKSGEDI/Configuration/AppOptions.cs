using System.ComponentModel.DataAnnotations;

namespace BACKSGEDI.Configuration;

public record AppOptions
{
    public const string SectionName = "Config";

    [Required(ErrorMessage = "La URL del Frontend es obligatoria para CORS.")]
    [Url(ErrorMessage = "La URL del Frontend no es válida.")]
    public string FrontendUrl { get; init; } = string.Empty;

    public string CookieDomain { get; init; } = string.Empty;

    [Required]
    public string AntiforgeryHeaderName { get; init; } = "X-XSRF-TOKEN";
}
