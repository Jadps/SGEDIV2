using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BACKSGEDI.Features.Alumnos.ToggleStatus;

public class ToggleStudentStatusEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    public ToggleStudentStatusEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/api/alumnos/{id}/toggle-status");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var usuario = await _db.Usuarios
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted && u.Alumno != null && u.Alumno.Id == id)
            .FirstOrDefaultAsync(ct);

        if (usuario is null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "Alumno no encontrado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }
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

        var isAdmin = roles.Any(r => r.Equals(SystemRoles.Admin, StringComparison.OrdinalIgnoreCase));
        var allowedCarreraIds = new HashSet<int>();
        if (!isAdmin)
        {
            if (roles.Any(r => r.Equals(SystemRoles.Coordinador, StringComparison.OrdinalIgnoreCase)))
            {
                var coordCarreras = await _db.Coordinadores
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.CarreraId)
                    .ToListAsync(ct);
                foreach (var cId in coordCarreras) allowedCarreraIds.Add(cId);
            }
        }
        var alumno = await _db.Alumnos.IgnoreQueryFilters().Where(a => a.Id == id && !a.IsDeleted).Select(a => new { a.CarreraId }).FirstOrDefaultAsync(ct);
        if (alumno is null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "Alumno no encontrado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }
        var isMyCareer = isAdmin || allowedCarreraIds.Contains(alumno.CarreraId);
        if (!isMyCareer)
        {
            await Result.Failure(Error.Forbidden("Auth.Forbidden", "No puedes modificar un alumno que no es de tu carrera."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }
        usuario.IsActive = !usuario.IsActive;
        await _db.SaveChangesAsync(ct);

        await Result<object>.Success(new { isActive = usuario.IsActive })
            .ToResult().ExecuteAsync(HttpContext);
    }
}