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
}

public class ListStudentEndpoint : Endpoint<ListStudentRequest, PagedResponse<AlumnoDto>>
{
    private readonly ApplicationDbContext _db;

    public ListStudentEndpoint(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/alumnos");
    }

    public override async Task HandleAsync(ListStudentRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var isAdmin = roles.Any(r => string.Equals(r, SystemRoles.Admin, StringComparison.OrdinalIgnoreCase));
        var isCoordinador = roles.Any(r => string.Equals(r, SystemRoles.Coordinador, StringComparison.OrdinalIgnoreCase));
        var isJefe = roles.Any(r => string.Equals(r, SystemRoles.JefeDepartamento, StringComparison.OrdinalIgnoreCase));

        var pageSize = req.GetSanitizedPageSize(maxPageSize: 50); 
        var skip = (req.Page - 1) * pageSize;
        
        var query = _db.Alumnos.AsNoTracking().AsQueryable();

        if (!isAdmin)
        {
            if (isCoordinador)
            {
                var carreraId = await _db.Coordinadores
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.CarreraId)
                    .FirstOrDefaultAsync(ct);
                
                if (carreraId != 0)
                    query = query.Where(a => a.CarreraId == carreraId);
            }
            else if (isJefe)
            {
                var carreraId = await _db.JefesDepartamento
                    .Where(j => j.UsuarioId == userId)
                    .Select(j => j.CarreraId)
                    .FirstOrDefaultAsync(ct);
                
                if (carreraId != 0)
                    query = query.Where(a => a.CarreraId == carreraId);
            }
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
                Carrera = _db.Carreras.Where(c => c.Id == a.CarreraId).Select(c => c.Nombre).FirstOrDefault() ?? "N/A",
                CreatedAt = a.Usuario.CreatedAt
            })
            .ToListAsync(ct);

        await Result<PagedResponse<AlumnoDto>>
            .Success(PagedResponse<AlumnoDto>.Create(items, totalCount, req.Page, pageSize))
            .ToResult()
            .ExecuteAsync(HttpContext);
    }
}
