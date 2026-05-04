using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

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
    public int Status { get; init; }

    public bool IsMyCareer { get; init; }
    public bool IsMyStudent { get; init; }
    public bool IsMyAdvisory { get; init; }
}

public class ListStudentEndpoint : Endpoint<ListStudentRequest, PagedResponse<AlumnoDto>>
{
    private readonly ApplicationDbContext _db;
    public ListStudentEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/alumnos");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento,
              SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno);
    }

    public override async Task HandleAsync(ListStudentRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var roles  = User.GetRoles();

        if (userId == Guid.Empty)
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var isAdmin     = roles.Contains(SystemRoles.Admin);
        var isCoord     = roles.Contains(SystemRoles.Coordinador);
        var isJefe      = roles.Contains(SystemRoles.JefeDepartamento);
        var isProfesor  = roles.Contains(SystemRoles.Profesor);
        var isAsesorInt = roles.Contains(SystemRoles.AsesorInterno);
        var isAsesorExt = roles.Contains(SystemRoles.AsesorExterno);

        int? callerCarreraId = null;
        Guid? callerAsesorInternoId = null;
        Guid? callerAsesorExternoId = null;
        HashSet<Guid> myStudentIds = [];

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
        {
            var profesorId = await _db.Profesores.Where(p => p.UsuarioId == userId).Select(p => (Guid?)p.Id).FirstOrDefaultAsync(ct);
            if (profesorId.HasValue)
            {
                var ids = await _db.CargasAcademicas
                    .Where(ca => ca.ProfesorId == profesorId.Value)
                    .Select(ca => ca.AlumnoId)
                    .Distinct()
                    .ToListAsync(ct);
                myStudentIds = [.. ids];
            }
        }

        var query = _db.Alumnos
            .AsNoTracking()
            .ApplySecurityFilter(userId, roles, _db);

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
        {
            var term = req.SearchTerm.ToLower();
            query = query.Where(a => a.Usuario.Name.ToLower().Contains(term) || a.Matricula.ToLower().Contains(term));
        }

        if (req.CarreraId.HasValue)
            query = query.Where(a => a.CarreraId == req.CarreraId.Value);

        var pagedResult = await query
            .OrderByDescending(a => a.Usuario.CreatedAt)
            .Select(a => new
            {
                a.Id,
                Name = a.Usuario.Name,
                Email = a.Usuario.Email,
                a.Matricula,
                Carrera = a.Carrera != null ? a.Carrera.Nombre : "N/A",
                CreatedAt = a.Usuario.CreatedAt,
                Status = a.Usuario.Status,
                a.CarreraId,
                a.AsesorInternoId,
                a.AsesorExternoId,
            })
            .ToPagedResponseAsync(req, ct);

        var items = pagedResult.Items.Select(a => new AlumnoDto
        {
            Id = a.Id,
            Name = a.Name,
            Email = a.Email,
            Matricula = a.Matricula,
            Carrera = a.Carrera,
            CreatedAt = a.CreatedAt,
            Status = a.Status,

            IsMyCareer = isAdmin
                || (isCoord && callerCarreraId == a.CarreraId)
                || (isJefe  && callerCarreraId == a.CarreraId),

            IsMyStudent = isAdmin || myStudentIds.Contains(a.Id),

            IsMyAdvisory = isAdmin
                || (isAsesorInt && callerAsesorInternoId.HasValue && a.AsesorInternoId == callerAsesorInternoId)
                || (isAsesorExt && callerAsesorExternoId.HasValue && a.AsesorExternoId == callerAsesorExternoId),
        }).ToList();

        var response = PagedResponse<AlumnoDto>.Create(items, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);

        await Result<PagedResponse<AlumnoDto>>.Success(response).ToResult().ExecuteAsync(HttpContext);
    }
}