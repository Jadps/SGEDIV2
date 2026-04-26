using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Plantillas;

public record PlantillaDto
{
    public int Id { get; init; }
    public int TipoDocumento { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public DateTime FechaSubida { get; init; }
}

public class ListPlantillas : EndpointWithoutRequest<List<PlantillaDto>>
{
    private readonly ApplicationDbContext _db;

    public ListPlantillas(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/plantillas");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, 
              SystemRoles.Alumno, SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plantillas = await _db.PlantillasDocumentos
            .AsNoTracking()
            .Where(p => p.EsVigente)
            .OrderBy(p => p.TipoDocumento)
            .Select(p => new PlantillaDto
            {
                Id = p.Id,
                TipoDocumento = (int)p.TipoDocumento,
                Tipo = p.TipoDocumento.ToString(),
                FechaSubida = p.FechaSubida
            })
            .ToListAsync(ct);

        await Result<List<PlantillaDto>>.Success(plantillas)
            .ToResult().ExecuteAsync(HttpContext);
    }
}

