using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Prorroga;

public record ExtendDeadlineRequest
{
    public Guid? AcuerdoId { get; init; }
    public Guid? AlumnoId { get; init; }
    public TipoAcuerdo? TipoAcuerdo { get; init; }
    public string? Semestre { get; init; }
    public DateTime NuevaFechaLimite { get; init; }
}

public class ExtendAcuerdoDeadline : Endpoint<ExtendDeadlineRequest>
{
    private readonly ApplicationDbContext _db;

    public ExtendAcuerdoDeadline(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/api/acuerdos/prorroga");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(ExtendDeadlineRequest req, CancellationToken ct)
    {
        DocumentoAcuerdo? acuerdo = null;

        if (req.AcuerdoId.HasValue)
        {
            acuerdo = await _db.DocumentosAcuerdos.FirstOrDefaultAsync(a => a.Id == req.AcuerdoId, ct);
        }
        else if (req.AlumnoId.HasValue && req.TipoAcuerdo.HasValue && !string.IsNullOrEmpty(req.Semestre))
        {
            acuerdo = await _db.DocumentosAcuerdos
                .FirstOrDefaultAsync(a => a.AlumnoId == req.AlumnoId && a.TipoAcuerdo == req.TipoAcuerdo && a.Semestre == req.Semestre && a.EsVersionActual, ct);

            if (acuerdo == null)
            {
                acuerdo = new DocumentoAcuerdo
                {
                    AlumnoId = req.AlumnoId.Value,
                    TipoAcuerdo = req.TipoAcuerdo.Value,
                    Semestre = req.Semestre,
                    FechaLimite = DateTime.SpecifyKind(req.NuevaFechaLimite, DateTimeKind.Utc),
                    Estado = EstadoDocumento.PendienteRevision,
                    Version = 1,
                    EsVersionActual = true,
                    SubidoPorUsuarioId = User.GetUserId()
                };
                _db.DocumentosAcuerdos.Add(acuerdo);
            }
        }

        if (acuerdo is null)
        {
            await Result.Failure(Error.NotFound("Acuerdo.NotFound", "No se pudo identificar el documento para otorgar la prórroga."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        acuerdo.FechaLimite = DateTime.SpecifyKind(req.NuevaFechaLimite, DateTimeKind.Utc);
        
        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
