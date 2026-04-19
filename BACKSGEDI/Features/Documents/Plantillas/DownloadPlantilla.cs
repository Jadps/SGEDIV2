using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;
namespace BACKSGEDI.Features.Documents.Plantillas;

public class DownloadPlantilla : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    private readonly string _basePath;

    public DownloadPlantilla(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _basePath = configuration["Storage:BasePath"] ?? "Uploads";
    }

    public override void Configure()
    {
        Get("/api/plantillas/{id}/download");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var plantilla = await _db.PlantillasDocumentos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (plantilla == null)
        {
            await Result.Failure(Error.NotFound("Plantilla.NotFound", "La plantilla no existe."))
    .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), _basePath, plantilla.RutaArchivo);

        if (!File.Exists(rutaAbsoluta))
        {
            await Result.Failure(Error.NotFound("Plantilla.FileNotFound", "El archivo físico no se encontró en el servidor.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Results.File(
            rutaAbsoluta,
            contentType: "application/pdf",
            fileDownloadName: plantilla.Nombre + ".pdf"
        ).ExecuteAsync(HttpContext);
    }
}
