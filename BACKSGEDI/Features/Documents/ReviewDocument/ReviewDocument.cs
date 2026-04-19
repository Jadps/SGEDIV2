using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BACKSGEDI.Features.Documents.ReviewDocument;

public class ReviewDocumentRequest
{
    public Guid DocumentoId { get; set; }
    public bool Aprobado { get; set; }
    public string? MotivoRechazo { get; set; }
}

public class ReviewDocument : Endpoint<ReviewDocumentRequest>
{
    private readonly ApplicationDbContext _db;

    public ReviewDocument(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Patch("/api/alumnos/{alumnoId}/documentos/{documentoId}/revisar");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(ReviewDocumentRequest req, CancellationToken ct)
    {
        var requesterId = User.GetUserId();
        var estado = req.Aprobado ? EstadoDocumento.Aprobado : EstadoDocumento.Rechazado;
        if (!req.Aprobado && string.IsNullOrWhiteSpace(req.MotivoRechazo))
        {
            await Result.Failure(Error.Validation("Doc.MotivoRequerido", "El motivo de rechazo es requerido."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value).ToList();

        var isAdmin = roles.Any(r => r.Equals(SystemRoles.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isAdmin)
        {
            var alumnoId = Route<Guid>("alumnoId");
            var carreraCoord = await _db.Coordinadores
                .Where(c => c.UsuarioId == requesterId)
                .Select(c => c.CarreraId)
                .FirstOrDefaultAsync(ct);

            var esDeMiCarrera = await _db.Alumnos
                .AnyAsync(a => a.Id == alumnoId && a.CarreraId == carreraCoord, ct);

            if (!esDeMiCarrera)
            {
                await Result.Failure(Error.Forbidden("Doc.Forbidden", "No tienes jurisdicción sobre este alumno."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }
        var personalDoc = await _db.DocumentosAlumnos.FirstOrDefaultAsync(d => d.Id == req.DocumentoId, ct);
        if (personalDoc != null)
        {
            personalDoc.Estado = estado;
            personalDoc.MotivoRechazo = req.MotivoRechazo;
            personalDoc.RevisadoPorUsuarioId = requesterId;
            personalDoc.FechaRevision = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            await Result.Success().ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var acuerdoDoc = await _db.DocumentosAcuerdos.FirstOrDefaultAsync(d => d.Id == req.DocumentoId, ct);
        if (acuerdoDoc != null)
        {
            acuerdoDoc.Estado = estado;
            acuerdoDoc.MotivoRechazo = req.MotivoRechazo;
            acuerdoDoc.RevisadoPorUsuarioId = requesterId;
            acuerdoDoc.FechaRevision = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            await Result.Success().ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Result.Failure(Error.NotFound("Doc.NotFound", "El documento no existe.")).ToResult().ExecuteAsync(HttpContext);
    }
}
