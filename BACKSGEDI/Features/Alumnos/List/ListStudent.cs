using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Features.Users.Me;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
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

    public int Status { get; init; }
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
    var userId = User.GetUserId();
    var roles = User.GetRoles();
    
    var isAdmin = roles.Contains(SystemRoles.Admin);

    if (userId == Guid.Empty)
    {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult()
                .ExecuteAsync(HttpContext);
            return;
    }

    var query = _db.Alumnos
        .AsNoTracking()
        .ApplySecurityFilter(userId, roles, _db); 

    if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
        var term = req.SearchTerm.ToLower();
        query = query.Where(a => a.Usuario.Name.ToLower().Contains(term) || a.Matricula.ToLower().Contains(term));
    }

    if (req.CarreraId.HasValue) {
        query = query.Where(a => a.CarreraId == req.CarreraId.Value);
    }

    var pagedResult = await query
        .OrderByDescending(a => a.Usuario.CreatedAt)
        .Select(a => new AlumnoDto
        {
            Id = a.Id,
            Name = a.Usuario.Name,
            Email = a.Usuario.Email,
            Matricula = a.Matricula,
            Carrera = a.Carrera != null ? a.Carrera.Nombre : "N/A",
            CreatedAt = a.Usuario.CreatedAt,
            Status = a.Usuario.Status,

            IsMyCareer = true,

            //IsMyStudent = isAdmin || _db.Alumnos.Any(am => am.Id == a.Id && am.ProfesorId == userId),
            //IsMyAdvisory = isAdmin || _db.Alumnos.Any(as => as.Id == a.Id && as.AsesorExternoId == userId),

            StatusText = "",
            StatusSeverity = ""
        })
        .ToPagedResponseAsync(req, ct);

    var finalItems = pagedResult.Items.Select(item => item with 
    {
        StatusText = StatusHelper.GetText(item.Status),
        StatusSeverity = StatusHelper.GetSeverity(item.Status, item.IsMyCareer)
    }).ToList();

    var finalResponse = PagedResponse<AlumnoDto>.Create(
        finalItems, 
        pagedResult.TotalCount, 
        pagedResult.Page, 
        pagedResult.PageSize);

    await Result<PagedResponse<AlumnoDto>>
        .Success(finalResponse)
        .ToResult()
        .ExecuteAsync(HttpContext);
    }
}