using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Extensions;
using System.Security.Claims;

namespace BACKSGEDI.Features.Alumnos.List;

public record ListStudentRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public int? CarreraId { get; init; }
}

public record AlumnoDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Matricula { get; init; } = string.Empty;
    public string Carrera { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public bool IsMyCareer { get; init; }
    public bool IsMyStudent { get; init; }
    public bool IsMyAdvisory { get; init; }

    public bool IsAccountActive { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public string StatusSeverity { get; init; } = "info";
}

public class ListStudentEndpoint : Endpoint<ListStudentRequest, PagedResponse<AlumnoDto>>
{
    private readonly ApplicationDbContext _db;

    public ListStudentEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/alumnos");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno);
    }

    public override async Task HandleAsync(ListStudentRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult()
                .ExecuteAsync(HttpContext);
            return;
        }

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var isAdmin = roles.Any(r => r.Equals(SystemRoles.Admin, StringComparison.OrdinalIgnoreCase));

        var allowedCarreraIds = new HashSet<int>();
        var allowedStudentIds = new HashSet<Guid>();

        if (!isAdmin)
        {
            if (roles.Any(r => r.Equals(SystemRoles.Coordinador, StringComparison.OrdinalIgnoreCase)))
            {
                var coordCarreras = await _db.Coordinadores
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.CarreraId)
                    .ToListAsync(ct);

                foreach (var id in coordCarreras) allowedCarreraIds.Add(id);
            }

            if (roles.Any(r => r.Equals(SystemRoles.JefeDepartamento, StringComparison.OrdinalIgnoreCase)))
            {
                var jefeCarreras = await _db.JefesDepartamento
                    .Where(j => j.UsuarioId == userId)
                    .Select(j => j.CarreraId)
                    .ToListAsync(ct);

                foreach (var id in jefeCarreras) allowedCarreraIds.Add(id);
            }

            //if (roles.Any(r => r.Equals(SystemRoles.Profesor, StringComparison.OrdinalIgnoreCase)))
            //{
            //    var alumnosMateria = await _db.AlumnoMaterias
            //        .Where(am => am.ProfesorId == userId)
            //        .Select(am => am.AlumnoId)
            //        .ToListAsync(ct);

            //    foreach (var id in alumnosMateria) allowedStudentIds.Add(id);
            //}

            //if (roles.Any(r => r.Equals(SystemRoles.AsesorInterno, StringComparison.OrdinalIgnoreCase)))
            //{
            //    var alumnosAsesor = await _db.Alumnos
            //        .Where(a => a.AsesorInternoId == userId)
            //        .Select(a => a.Id)
            //        .ToListAsync(ct);

            //    foreach (var id in alumnosAsesor) allowedStudentIds.Add(id);
            //}
        }

        var query = _db.Alumnos.AsNoTracking().AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(a =>
                allowedCarreraIds.Contains(a.CarreraId) ||
                allowedStudentIds.Contains(a.Id));
        }

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
        {
            var term = req.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Usuario.Name.ToLower().Contains(term) ||
                a.Matricula.ToLower().Contains(term) ||
                a.Usuario.Email.ToLower().Contains(term));
        }

        if (req.CarreraId.HasValue)
        {
            query = query.Where(a => a.CarreraId == req.CarreraId.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var pageSize = req.GetSanitizedPageSize(maxPageSize: 50);
        var skip = (req.Page - 1) * pageSize;

        var items = await query
            .OrderByDescending(a => a.Usuario.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(a => new AlumnoDto
            {
                Id = a.Id,
                Name = a.Usuario.Name,
                Email = a.Usuario.Email,
                Matricula = a.Matricula,
                Carrera = _db.Carreras
                    .Where(c => c.Id == a.CarreraId)
                    .Select(c => c.Nombre)
                    .FirstOrDefault() ?? "N/A",
                CreatedAt = a.Usuario.CreatedAt,
                IsAccountActive = a.Usuario.IsActive,
                IsMyCareer = allowedCarreraIds.Contains(a.CarreraId),
                IsMyStudent = allowedStudentIds.Contains(a.Id),
                IsMyAdvisory = allowedStudentIds.Contains(a.Id),
                StatusText = StatusHelper.GetText(a.Usuario.IsActive),
                StatusSeverity = StatusHelper.GetSeverity(a.Usuario.IsActive, allowedCarreraIds.Contains(a.CarreraId))
            })
            .ToListAsync(ct);

        await Result<PagedResponse<AlumnoDto>>
            .Success(PagedResponse<AlumnoDto>.Create(items, totalCount, req.Page, pageSize))
            .ToResult()
            .ExecuteAsync(HttpContext);
    }
}