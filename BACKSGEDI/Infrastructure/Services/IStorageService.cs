using Microsoft.AspNetCore.Http;

namespace BACKSGEDI.Infrastructure.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string alumnoId, string tipoDocumento, string semestre, CancellationToken ct = default);
    Task<string> UploadPlantillaAsync(IFormFile file, string tipoPlantilla, CancellationToken ct = default);
    void DeleteStudentFolder(string alumnoId);
}
