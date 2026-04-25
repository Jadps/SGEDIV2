using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Plantillas;

public class DeletePlantilla : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;

    public DeletePlantilla(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/api/plantillas/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var plantilla = await _db.PlantillasDocumentos.FirstOrDefaultAsync(p => p.Id == id, ct);
        
        if (plantilla == null)
        {
            await Result.Failure(Error.NotFound("Plantilla.NotFound", "Plantilla no encontrada."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        plantilla.IsDeleted = true;
        plantilla.EsVigente = false;
        
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
