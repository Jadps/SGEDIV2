using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;

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
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListStudentRequest req, CancellationToken ct)
    {
        var pageSize = req.GetSanitizedPageSize(maxPageSize: 50); 
        var skip = (req.Page - 1) * pageSize;
        var query = _db.Alumnos.AsNoTracking().AsQueryable();

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
                Carrera = "Carrera " + a.CarreraId, 
                CreatedAt = a.Usuario.CreatedAt
            })
            .ToListAsync(ct);

        await SendAsync(PagedResponse<AlumnoDto>.Create(items, totalCount, req.Page, pageSize), cancellation: ct);
    }
}
