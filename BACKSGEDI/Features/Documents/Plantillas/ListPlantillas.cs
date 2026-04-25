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
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; }
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
                Tipo = p.TipoDocumento.ToString(),
                Nombre = p.Nombre,
                FechaSubida = p.FechaSubida
            })
            .ToListAsync(ct);

        await Result<List<PlantillaDto>>.Success(plantillas)
            .ToResult().ExecuteAsync(HttpContext);
    }
}

