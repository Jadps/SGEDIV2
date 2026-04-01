using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BACKSGEDI.Infrastructure.Services;

public class LocalFileStorageService : IStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _basePath;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _basePath = configuration["Storage:BasePath"] ?? "Uploads";
    }

    public async Task<string> UploadFileAsync(IFormFile file, string userId, string folderType, CancellationToken ct = default)
    {
        var relativePath = Path.Combine("Students", userId, folderType);
        
        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), _basePath, relativePath);

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
        }

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        var filePath = Path.Combine(absolutePath, uniqueFileName);
        var dbRelativePath = Path.Combine(relativePath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        _logger.LogInformation("Archivo {FileName} guardado exitosamente en {Path}", file.FileName, filePath);

        return dbRelativePath;
    }
}
