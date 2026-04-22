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
            var userId = User.GetUserId();
            var roles = User.GetRoles();

            if (userId == Guid.Empty)
            {
                await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }

            var isAdmin = roles.Contains(SystemRoles.Admin);

            var dto = await _db.Alumnos
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(a => a.Id == id && !a.IsDeleted && !a.Usuario.IsDeleted)
                .ApplySecurityFilter(userId, roles, _db)
                .Select(a => new AlumnoDetailDto
                {
                    Id = a.Id,
                    Name = a.Usuario.Name,
                    Email = a.Usuario.Email,
                    Matricula = a.Matricula,
                    Carrera = a.Carrera != null ? a.Carrera.Nombre : "N/A",
                    Semestre = $"Semestre {a.SemestreId}",
                    CreatedAt = a.Usuario.CreatedAt,
                    IsAccountActive = a.Usuario.IsActive,

                    IsAdmin = isAdmin,

                    IsMyCareer = isAdmin ||
                                 _db.Coordinadores.Any(c => c.UsuarioId == userId && c.CarreraId == a.CarreraId) ||
                                 _db.JefesDepartamento.Any(j => j.UsuarioId == userId && j.CarreraId == a.CarreraId),

                    IsMyStudent = false,
                    IsMyAdvisory = false,

                    StatusText = "",
                    StatusSeverity = ""
                })
                .FirstOrDefaultAsync(ct);

            if (dto is null)
            {
                await Result.Failure(Error.NotFound("Alumno.NotFound", "Alumno no encontrado."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }

            var final = dto with
            {
                StatusText = StatusHelper.GetText(dto.IsAccountActive),
                StatusSeverity = StatusHelper.GetSeverity(dto.IsAccountActive, dto.IsMyCareer)
            };

            await Result<AlumnoDetailDto>.Success(final)
                .ToResult().ExecuteAsync(HttpContext);
        }
    }
