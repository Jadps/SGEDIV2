using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Contratos;

public record RespondToContratoRequest
{
    public Guid ContratoId { get; init; }
    public bool Aceptado { get; init; }
    public string? Observaciones { get; init; }
}

public class RespondToContrato : Endpoint<RespondToContratoRequest>
{
    private readonly ApplicationDbContext _db;

    public RespondToContrato(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Patch("/api/contratos/{contratoId}/responder");
        Roles(SystemRoles.Alumno, SystemRoles.Admin);
    }

    public override async Task HandleAsync(RespondToContratoRequest req, CancellationToken ct)
    {
        var contratoId = Route<Guid>("contratoId");
        var contrato = await _db.ContratosProfesores
            .Include(a => a.Alumno)
            .FirstOrDefaultAsync(a => a.Id == contratoId, ct);

        if (contrato == null)
        {
            await Result.Failure(Error.NotFound("Contrato.NotFound", "El contrato no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var userId = User.GetUserId();
        if (!User.IsInRole(SystemRoles.Admin) && contrato.Alumno?.UsuarioId != userId)
        {
            await Result.Failure(Error.Forbidden("Contrato.Forbidden", "No tienes permiso para responder a este contrato."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (contrato.Estado == EstadoContrato.Aceptado)
        {
            await Result.Failure(Error.Conflict("Contrato.Immutable", "Este contrato ya ha sido aceptado y no puede modificarse."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        contrato.Estado = req.Aceptado ? EstadoContrato.Aceptado : EstadoContrato.Rechazado;
        contrato.MotivoRechazo = req.Aceptado ? null : req.Observaciones;
        contrato.FechaAceptacion = req.Aceptado ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
