using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Domain.Common;

namespace BACKSGEDI.Features.Alumnos.Get;

public record AlumnoDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Matricula { get; init; } = string.Empty;
    public string Carrera { get; init; } = string.Empty;
    public string Semestre { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }

    public bool IsAdmin { get; init; }
    public bool IsMyCareer { get; init; }
    public bool IsMyStudent { get; init; }
    public bool IsMyAdvisory { get; init; }

    public string? AsesorInternoNombre { get; init; }
    public string? AsesorExternoNombre { get; init; }
    public string? EmpresaNombre { get; init; }
}

public class GetStudentEndpoint : EndpointWithoutRequest<AlumnoDetailDto>
{
    private readonly ApplicationDbContext _db;
    public GetStudentEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/alumnos/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento,
              SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno, SystemRoles.Alumno);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var userId = User.GetUserId();
        var roles = User.GetRoles();

        if (userId == Guid.Empty)
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var isAdmin        = roles.Contains(SystemRoles.Admin);
        var isCoord        = roles.Contains(SystemRoles.Coordinador);
        var isJefe         = roles.Contains(SystemRoles.JefeDepartamento);
        var isProfesor     = roles.Contains(SystemRoles.Profesor);
        var isAsesorInt    = roles.Contains(SystemRoles.AsesorInterno);
        var isAsesorExt    = roles.Contains(SystemRoles.AsesorExterno);

        int? callerCarreraId = null;
        Guid? callerAsesorInternoId = null;
        Guid? callerAsesorExternoId = null;
        Guid? callerProfesorId = null;

        if (isCoord || isJefe)
        {
            callerCarreraId = isCoord
                ? await _db.Coordinadores.Where(c => c.UsuarioId == userId).Select(c => (int?)c.CarreraId).FirstOrDefaultAsync(ct)
                : await _db.JefesDepartamento.Where(j => j.UsuarioId == userId).Select(j => (int?)j.CarreraId).FirstOrDefaultAsync(ct);
        }
        if (isAsesorInt)
            callerAsesorInternoId = await _db.AsesoresInternos.Where(a => a.UsuarioId == userId).Select(a => (Guid?)a.Id).FirstOrDefaultAsync(ct);
        if (isAsesorExt)
            callerAsesorExternoId = await _db.AsesoresExternos.Where(a => a.UsuarioId == userId).Select(a => (Guid?)a.Id).FirstOrDefaultAsync(ct);
        if (isProfesor)
            callerProfesorId = await _db.Profesores.Where(p => p.UsuarioId == userId).Select(p => (Guid?)p.Id).FirstOrDefaultAsync(ct);

        var alumno = await _db.Alumnos
            .AsNoTracking()
            .Where(a => a.Id == id)
            .ApplySecurityFilter(userId, roles, _db)
            .Select(a => new
            {
                a.Id,
                Name = a.Usuario.Name,
                Email = a.Usuario.Email,
                a.Matricula,
                Carrera = a.Carrera != null ? a.Carrera.Nombre : "N/A",
                Semestre = $"Semestre {a.SemestreId}",
                CreatedAt = a.Usuario.CreatedAt,
                Status = a.Usuario.Status,
                a.CarreraId,
                a.AsesorInternoId,
                a.AsesorExternoId,
                AsesorInternoNombre = a.AsesorInterno != null ? a.AsesorInterno.Usuario.Name : null,
                AsesorExternoNombre = a.AsesorExterno != null ? a.AsesorExterno.Usuario.Name : null,
                EmpresaNombre = a.Empresa != null ? a.Empresa.Nombre : null,
            })
            .FirstOrDefaultAsync(ct);

        if (alumno is null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "Alumno no encontrado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var isMyCareer = isAdmin
            || (isCoord && callerCarreraId == alumno.CarreraId)
            || (isJefe  && callerCarreraId == alumno.CarreraId);

        var isMyAdvisory = isAdmin
            || (isAsesorInt && callerAsesorInternoId.HasValue && alumno.AsesorInternoId == callerAsesorInternoId)
            || (isAsesorExt && callerAsesorExternoId.HasValue && alumno.AsesorExternoId == callerAsesorExternoId);

        var isMyStudent = isAdmin;
        if (!isMyStudent && isProfesor && callerProfesorId.HasValue)
            isMyStudent = await _db.CargasAcademicas
                .AnyAsync(ca => ca.AlumnoId == alumno.Id && ca.ProfesorId == callerProfesorId.Value, ct);

        var dto = new AlumnoDetailDto
        {
            Id = alumno.Id,
            Name = alumno.Name,
            Email = alumno.Email,
            Matricula = alumno.Matricula,
            Carrera = alumno.Carrera,
            Semestre = alumno.Semestre,
            CreatedAt = alumno.CreatedAt,
            Status = alumno.Status,
            IsAdmin = isAdmin,
            IsMyCareer = isMyCareer,
            IsMyStudent = isMyStudent,
            IsMyAdvisory = isMyAdvisory,
            AsesorInternoNombre = alumno.AsesorInternoNombre,
            AsesorExternoNombre = alumno.AsesorExternoNombre,
            EmpresaNombre = alumno.EmpresaNombre,
        };

        await Result<AlumnoDetailDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}
