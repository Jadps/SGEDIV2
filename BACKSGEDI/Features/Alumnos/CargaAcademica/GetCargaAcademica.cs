using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Features.Alumnos.Get;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Alumnos.CargaAcademica;

public record CargaAcademicaDto(
    Guid Id, 
    Guid MateriaId, string MateriaNombre, string MateriaClave, int MateriaCreditos,
    Guid ProfesorId, string ProfesorNombre, string ProfesorEmail, string? ProfesorNumeroEmpleado, string? cubiculoProfesor,
    string Semestre);

public class GetCargaAcademica : EndpointWithoutRequest<List<CargaAcademicaDto>>
{
    private readonly ApplicationDbContext _db;

    public GetCargaAcademica(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/alumnos/{alumnoId}/carga-academica");
        Roles(SystemRoles.Alumno, SystemRoles.Coordinador, SystemRoles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var alumnoIdStr = Route<string>("alumnoId");
        Guid targetAlumnoId;

        if (alumnoIdStr == "me")
        {
            var userId = User.GetUserId();
            var alumno = await _db.Alumnos.FirstOrDefaultAsync(a => a.UsuarioId == userId, ct);
            if (alumno == null)
            {
                await Result.Failure(Error.NotFound("Alumno.NotFound", "No se encontró el perfil del alumno.")).ToResult().ExecuteAsync(HttpContext);
                return;
            }
            targetAlumnoId = alumno.Id;
        }
        else
        {
            if (!Guid.TryParse(alumnoIdStr, out targetAlumnoId))
            {
                await Result.Failure(Error.Validation("Alumno.InvalidId", "ID de alumno inválido.")).ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }

        var semestreActual = SemestreHelper.GetSemestreActual();

        var carga = await _db.CargasAcademicas
            .Where(ca => ca.AlumnoId == targetAlumnoId && ca.Semestre == semestreActual)
            .Select(ca => new CargaAcademicaDto(
                ca.Id,
                ca.MateriaId,
                ca.Materia!.Nombre,
                ca.Materia!.Clave,
                ca.Materia!.Creditos,
                ca.ProfesorId,
                ca.Profesor!.Usuario!.Name,
                ca.Profesor!.Usuario!.Email,
                ca.Profesor!.NumeroEmpleado,
                ca.Profesor!.Cubiculo,
                ca.Semestre
            ))
            .ToListAsync(ct);

        await Result<List<CargaAcademicaDto>>.Success(carga)
    .ToResult().ExecuteAsync(HttpContext);
    }
}
