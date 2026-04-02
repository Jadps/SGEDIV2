namespace BACKSGEDI.Configuration;

public record AdminOptions
{
    public const string SectionName = "Adminconfig";
    public string Mail { get; init; } = string.Empty;
    public string Pass { get; init; } = string.Empty;
}
