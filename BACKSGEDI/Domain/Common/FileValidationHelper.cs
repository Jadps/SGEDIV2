using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace BACKSGEDI.Domain.Common;

public static class FileValidationHelper
{
    public static bool IsValidPdf(IFormFile? file, long maxSize)
    {
        if (file == null) return false;
        
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isPdf = ext == ".pdf" && file.ContentType == "application/pdf";

        return isPdf && file.Length <= maxSize;
    }

    public static bool IsValidWord(IFormFile? file, long maxSize)
    {
        if (file == null) return false;
        
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isWord = (ext == ".doc" || ext == ".docx") && 
                     (file.ContentType == "application/msword" || 
                      file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        return isWord && file.Length <= maxSize;
    }

    public static bool IsValidPdfOrWord(IFormFile? file, long maxSize)
    {
        return IsValidPdf(file, maxSize) || IsValidWord(file, maxSize);
    }
}
