using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Domain.Common;
using System.Security.Claims;

namespace BACKSGEDI.Features.Alumnos.Get;

public record AlumnoDocumentoDto
{
    public string Tipo { get; init; } = string.Empty;
    public DateTime FechaSubida { get; init; }
    public string Ruta { get; init; } = string.Empty;
}

public record AlumnoDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Matricula { get; init; } = string.Empty;
    public string Carrera { get; init; } = string.Empty;
    public string Semestre { get; init; } = string.Empty;
    public bool IsAccountActive { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public string StatusSeverity { get; init; } = "info";
    public DateTime CreatedAt { get; init; }
    
    public bool IsMyCareer { get; init; }
    public bool IsMyStudent { get; init; }
    public bool IsMyAdvisory { get; init; }
    public bool IsAdmin { get; init; }
}

public class GetStudentEndpoint : EndpointWithoutRequest<AlumnoDetailDto>
{
    private readonly ApplicationDbContext _db;

    public GetStudentEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/alumnos/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var isAdminRole = roles.Any(r => r.Equals(SystemRoles.Admin, StringComparison.OrdinalIgnoreCase));
        
        var allowedCarreraIds = new HashSet<int>();
        if (!isAdminRole)
        {
            if (roles.Any(r => r.Equals(SystemRoles.Coordinador, StringComparison.OrdinalIgnoreCase)))
            {
                var coordCarreras = await _db.Coordinadores
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.CarreraId)
                    .ToListAsync(ct);
                foreach (var cId in coordCarreras) allowedCarreraIds.Add(cId);
            }

            if (roles.Any(r => r.Equals(SystemRoles.JefeDepartamento, StringComparison.OrdinalIgnoreCase)))
            {
                var jefeCarreras = await _db.JefesDepartamento
                    .Where(j => j.UsuarioId == userId)
                    .Select(j => j.CarreraId)
                    .ToListAsync(ct);
                foreach (var cId in jefeCarreras) allowedCarreraIds.Add(cId);
            }
        }

        var alumno = await _db.Alumnos
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Include(a => a.Documentos)
            .Where(a => a.Id == id && !a.IsDeleted && !a.Usuario.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (alumno is null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "Alumno no encontrado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var isMyCareer = isAdminRole || allowedCarreraIds.Contains(alumno.CarreraId);
        var isMyStudent = false; 
        var isMyAdvisory = false;

        var dto = new AlumnoDetailDto
        {
            Id = alumno.Id,
            Name = alumno.Usuario.Name,
            Email = alumno.Usuario.Email,
            Matricula = alumno.Matricula,
            Carrera = await _db.Carreras
                .Where(c => c.Id == alumno.CarreraId)
                .Select(c => c.Nombre)
                .FirstOrDefaultAsync(ct) ?? "N/A",
            Semestre = $"Semestre {alumno.SemestreId}",
            IsAccountActive = alumno.Usuario.IsActive,
            StatusText = StatusHelper.GetText(alumno.Usuario.IsActive),
            StatusSeverity = StatusHelper.GetSeverity(alumno.Usuario.IsActive, isMyCareer),
            CreatedAt = alumno.Usuario.CreatedAt,
            IsMyCareer = isMyCareer,
            IsMyStudent = isMyStudent,
            IsMyAdvisory = isMyAdvisory,
            IsAdmin = isAdminRole
        };


        await Result<AlumnoDetailDto>.Success(dto)
            .ToResult().ExecuteAsync(HttpContext);
    }
}
