using Microsoft.AspNetCore.Http;

namespace BACKSGEDI.Infrastructure.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string userId, string folderType, CancellationToken ct = default);
}
