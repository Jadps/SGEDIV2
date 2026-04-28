using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BACKSGEDI.Configuration;
using Microsoft.Extensions.Options;

namespace BACKSGEDI.Infrastructure.Services;

public class LocalFileStorageService : IStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _basePath;

    public string BasePath => _basePath;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger, IOptions<StorageOptions> options)
    {
        _logger = logger;
        _basePath = options.Value.BasePath;    
    }

    public async Task<string> UploadFileAsync(IFormFile file, string alumnoId, string tipoDocumento, string semestre, CancellationToken ct = default)
    {
        var relativePath = Path.Combine("Students", alumnoId, semestre);
        var absolutePath = Path.Combine(_basePath, relativePath);

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
        }

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{tipoDocumento}_{Guid.NewGuid()}{extension}";
        
        var filePath = Path.Combine(absolutePath, uniqueFileName);
        var dbRelativePath = Path.Combine(relativePath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        _logger.LogInformation("Archivo {FileName} guardado exitosamente en {Path}", file.FileName, filePath);

        return dbRelativePath;
    }

    public async Task<string> UploadPlantillaAsync(IFormFile file, string tipoPlantilla, CancellationToken ct = default)
    {
        var relativePath = Path.Combine("Plantillas", tipoPlantilla);
        var absolutePath = Path.Combine(_basePath, relativePath);

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

        _logger.LogInformation("Plantilla {FileName} guardada exitosamente en {Path}", file.FileName, filePath);

        return dbRelativePath;
    }

    public void DeleteStudentFolder(string alumnoId)
    {
        var folderPath = Path.Combine(_basePath, "Students", alumnoId);

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, recursive: true);
            _logger.LogWarning("Rollback: carpeta {Folder} eliminada del disco.", folderPath);
        }
    }
}
