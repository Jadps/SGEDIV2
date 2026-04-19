using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Acuerdos;

public class CreateAcuerdoRequest
{
    public Guid AlumnoId { get; set; }
    public TipoAcuerdo TipoAcuerdo { get; set; }
    public Guid? ProfesorId { get; set; }
    public Guid? AsesorInternoId { get; set; }
    public Guid? AsesorExternoId { get; set; }
    public DateTime? FechaLimiteManual { get; set; }
}

public record AcuerdoDto
{
    public Guid Id { get; init; }
    public Guid AlumnoId { get; init; }
    public TipoAcuerdo TipoAcuerdo { get; init; }
    public string Semestre { get; init; } = string.Empty;
    public DateTime? FechaLimite { get; init; }
    public EstadoDocumento Estado { get; init; }
}

public class CreateAcuerdo : Endpoint<CreateAcuerdoRequest>
{
    private readonly ApplicationDbContext _db;

    public CreateAcuerdo(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/acuerdos");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CreateAcuerdoRequest req, CancellationToken ct)
    {
        var requesterId = User.GetUserId();
        var semestreActual = SemestreHelper.GetSemestreActual();

        var alumnoExiste = await _db.Alumnos.AnyAsync(a => a.Id == req.AlumnoId, ct);
        if (!alumnoExiste)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "El alumno no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }
        var acuerdoExiste = await _db.DocumentosAcuerdos
            .AnyAsync(a => a.AlumnoId == req.AlumnoId 
               && a.TipoAcuerdo == req.TipoAcuerdo 
               && a.Semestre == semestreActual
               && a.EsVersionActual, ct);

        if (acuerdoExiste)
        {
            await Result.Failure(Error.Conflict("Acuerdo.AlreadyExists", 
                "Ya existe un acuerdo de este tipo para el alumno en el semestre actual."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }
        var acuerdo = new DocumentoAcuerdo
        {
            AlumnoId = req.AlumnoId,
            TipoAcuerdo = req.TipoAcuerdo,
            ProfesorId = req.ProfesorId,
            AsesorInternoId = req.AsesorInternoId,
            AsesorExternoId = req.AsesorExternoId,
            Semestre = semestreActual,
            FechaLimite = req.FechaLimiteManual ?? FechasLimiteService.GetDefaultFechaLimite(semestreActual),
            Estado = EstadoDocumento.PendienteRevision,
            SubidoPorUsuarioId = requesterId,
            Version = 1,
            EsVersionActual = true
        };

        await _db.DocumentosAcuerdos.AddAsync(acuerdo, ct);
        await _db.SaveChangesAsync(ct);

        await Result<AcuerdoDto>.Success(new AcuerdoDto
        {
            Id = acuerdo.Id,
            AlumnoId = acuerdo.AlumnoId,
            TipoAcuerdo = acuerdo.TipoAcuerdo,
            Semestre = acuerdo.Semestre,
            FechaLimite = acuerdo.FechaLimite,
            Estado = acuerdo.Estado
        }).ToResult().ExecuteAsync(HttpContext);
    }
}