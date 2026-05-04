using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Evaluaciones;

public record EvaluacionDto
{
    public Guid Id { get; init; }
    public string EvaluadorNombre { get; init; } = string.Empty;
    public string EvaluadorRol { get; init; } = string.Empty;
    public int Calificacion { get; init; }
    public string Observaciones { get; init; } = string.Empty;
    public string Semestre { get; init; } = string.Empty;
    public DateTime FechaEvaluacion { get; init; }
}

public class GetEvaluaciones : EndpointWithoutRequest<List<EvaluacionDto>>
{
    private readonly ApplicationDbContext _db;

    public GetEvaluaciones(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/evaluaciones/{alumnoId}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno, SystemRoles.Alumno);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var alumnoId = Route<Guid>("alumnoId");
        var userId = User.GetUserId();
        var roles = User.GetRoles();
        var isAdmin = roles.Contains(SystemRoles.Admin);
        var isCoord = roles.Contains(SystemRoles.Coordinador);

        if (!isAdmin && !isCoord)
        {
            var isMe = roles.Contains(SystemRoles.Alumno) && await _db.Alumnos.AnyAsync(a => a.Id == alumnoId && a.UsuarioId == userId, ct);
            var isAdvisor = (roles.Contains(SystemRoles.AsesorInterno) || roles.Contains(SystemRoles.AsesorExterno)) && 
                            await _db.Alumnos.AnyAsync(a => a.Id == alumnoId && (a.AsesorInterno.UsuarioId == userId || a.AsesorExterno.UsuarioId == userId), ct);

            if (!isMe && !isAdvisor)
            {
                await Result.Failure(Error.Forbidden("Eval.Forbidden", "No tienes acceso a estas evaluaciones."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }

        var evaluaciones = await _db.EvaluacionesDesempeño
            .AsNoTracking()
            .Where(e => e.AlumnoId == alumnoId)
            .OrderByDescending(e => e.FechaEvaluacion)
            .Select(e => new EvaluacionDto
            {
                Id = e.Id,
                EvaluadorNombre = e.Evaluador.Name,
                EvaluadorRol = _db.UsuariosRoles.Where(ur => ur.UsuarioId == e.EvaluadorId).Select(ur => ur.Role).FirstOrDefault() ?? "Desconocido",
                Calificacion = e.Calificacion,
                Observaciones = e.Observaciones,
                Semestre = e.Semestre,
                FechaEvaluacion = e.FechaEvaluacion
            })
            .ToListAsync(ct);

        await Result<List<EvaluacionDto>>.Success(evaluaciones).ToResult().ExecuteAsync(HttpContext);
    }
}
