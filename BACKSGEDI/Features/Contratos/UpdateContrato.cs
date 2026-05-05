using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Contratos;

public record UpdateContratoRequest
{
    public Guid Id { get; init; }
    public ModalidadContrato Modalidad { get; init; }
    public string? Descripcion { get; init; }
    public List<CreateCriterioDto> Criterios { get; init; } = new();
}

public class UpdateContrato : Endpoint<UpdateContratoRequest>
{
    private readonly ApplicationDbContext _db;

    public UpdateContrato(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put("/api/contratos/{id}");
        Roles(SystemRoles.Profesor, SystemRoles.Admin);
    }

    public override async Task HandleAsync(UpdateContratoRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        
        var contrato = await _db.ContratosProfesores
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (contrato == null)
        {
            await Result.Failure(Error.NotFound("Contrato.NotFound", "El contrato no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        contrato.Modalidad = req.Modalidad;
        contrato.Descripcion = req.Descripcion;
        contrato.Estado = EstadoContrato.Pendiente;
        contrato.MotivoRechazo = null;

        var criteriosExistentes = await _db.CriteriosEvaluacion
            .IgnoreQueryFilters()
            .Where(c => c.ContratoId == id)
            .ToListAsync(ct);

        _db.CriteriosEvaluacion.RemoveRange(criteriosExistentes);

        foreach (var c in req.Criterios)
        {
            _db.CriteriosEvaluacion.Add(new CriterioEvaluacion
            {
                ContratoId = id,
                Tipo = c.Tipo,
                Detalle = c.Detalle,
                Porcentaje = c.Porcentaje
            });
        }

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
