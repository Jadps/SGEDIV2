namespace BACKSGEDI.Configuration;

public record StorageOptions
{
    public const string SectionName = "Storage";

    public string BasePath { get; init; } = "Uploads";
    public long MaxFileSizeInBytes { get; init; } = 5242880;
}