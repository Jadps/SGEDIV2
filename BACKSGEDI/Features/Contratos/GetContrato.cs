using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Contratos;

public record GetContratoRequest
{
    public Guid AlumnoId { get; init; }
    public Guid MateriaId { get; init; }
}

public record ContratoResponse
{
    public Guid Id { get; init; }
    public Guid MateriaId { get; init; }
    public string MateriaNombre { get; init; } = string.Empty;
    public Guid AlumnoId { get; init; }
    public string AlumnoNombre { get; init; } = string.Empty;
    public Guid ProfesorId { get; init; }
    public string ProfesorNombre { get; init; } = string.Empty;
    public int Modalidad { get; init; }
    public string ModalidadLabel { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string? MotivoRechazo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public List<CriterioDto> Criterios { get; init; } = new();
}

public record CriterioDto
{
    public Guid Id { get; init; }
    public int Tipo { get; init; }
    public string TipoLabel { get; init; } = string.Empty;
    public string Detalle { get; init; } = string.Empty;
    public int Porcentaje { get; init; }
}

public class GetContrato : Endpoint<GetContratoRequest>
{
    private readonly ApplicationDbContext _db;

    public GetContrato(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/contratos/{alumnoId}/{materiaId}");
        Roles(SystemRoles.Profesor, SystemRoles.Alumno, SystemRoles.Coordinador, SystemRoles.Admin);
    }

    public override async Task HandleAsync(GetContratoRequest req, CancellationToken ct)
    {
        var contrato = await _db.ContratosProfesores
            .Include(a => a.Materia)
            .Include(a => a.Alumno).ThenInclude(a => a!.Usuario)
            .Include(a => a.Profesor).ThenInclude(p => p!.Usuario)
            .Include(a => a.Criterios)
            .FirstOrDefaultAsync(a => a.AlumnoId == req.AlumnoId && a.MateriaId == req.MateriaId, ct);

        if (contrato == null)
        {
            await Result.Failure(Error.NotFound("Contrato.NotFound", "El contrato no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var response = new ContratoResponse
        {
            Id = contrato.Id,
            MateriaId = contrato.MateriaId,
            MateriaNombre = contrato.Materia?.Nombre ?? string.Empty,
            AlumnoId = contrato.AlumnoId,
            AlumnoNombre = contrato.Alumno?.Usuario?.Name ?? string.Empty,
            ProfesorId = contrato.ProfesorId,
            ProfesorNombre = contrato.Profesor?.Usuario?.Name ?? string.Empty,
            Modalidad = (int)contrato.Modalidad,
            ModalidadLabel = contrato.Modalidad.ToString(),
            Descripcion = contrato.Descripcion,
            Estado = contrato.Estado.ToString(),
            MotivoRechazo = contrato.MotivoRechazo,
            FechaCreacion = contrato.FechaCreacion,
            Criterios = contrato.Criterios.Select(c => new CriterioDto
            {
                Id = c.Id,
                Tipo = (int)c.Tipo,
                TipoLabel = c.Tipo.ToString(),
                Detalle = c.Detalle,
                Porcentaje = c.Porcentaje
            }).ToList()
        };

        await Result<ContratoResponse>.Success(response).ToResult().ExecuteAsync(HttpContext);
    }
}
