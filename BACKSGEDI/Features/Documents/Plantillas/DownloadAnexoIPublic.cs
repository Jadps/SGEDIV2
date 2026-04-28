using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Plantillas;

public class DownloadAnexoIPublic : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    private readonly string _basePath;

    public DownloadAnexoIPublic(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _basePath = configuration["Storage:BasePath"] ?? "Uploads";
    }

    public override void Configure()
    {
        Get("/api/plantillas/public/anexo-i");
        AllowAnonymous();
        Throttle(10, 60);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plantilla = await _db.PlantillasDocumentos
            .AsNoTracking()
            .Where(p => p.TipoDocumento == TipoPlantilla.AnexoI && p.EsVigente)
            .OrderByDescending(p => p.FechaSubida)
            .FirstOrDefaultAsync(ct);

        if (plantilla == null)
        {
            await Result.Failure(Error.NotFound("Plantilla.NotFound", "No hay una plantilla vigente para el Anexo I."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), _basePath, plantilla.RutaArchivo);

        if (!File.Exists(rutaAbsoluta))
        {
            await Result.Failure(Error.NotFound("Plantilla.FileNotFound", "El archivo físico no se encuentra en el servidor."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var ext = Path.GetExtension(plantilla.RutaArchivo).ToLowerInvariant();
        var contentType = ext switch
        {
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/pdf"
        };

        await Results.File(
            rutaAbsoluta,
            contentType: contentType,
            fileDownloadName: "Anexo_I_Oficial" + ext
        ).ExecuteAsync(HttpContext);
    }
}
