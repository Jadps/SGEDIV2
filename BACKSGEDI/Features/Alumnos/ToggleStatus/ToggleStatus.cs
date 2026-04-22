using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

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
        var userId = User.GetUserId();
        var roles = User.GetRoles();

        if (userId == Guid.Empty)
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var tienePermiso = await _db.Alumnos
            .IgnoreQueryFilters()
            .ApplySecurityFilter(userId, roles, _db)
            .AnyAsync(a => a.Id == id && !a.IsDeleted, ct);

        if (!tienePermiso)
        {
            await Result.Failure(Error.Forbidden("Auth.Forbidden", "No tienes permiso para modificar a este alumno."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var usuario = await _db.Usuarios
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted && u.Alumno != null && u.Alumno.Id == id)
            .FirstOrDefaultAsync(ct);

        if (usuario is null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "El usuario asociado no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        usuario.IsActive = !usuario.IsActive;
        await _db.SaveChangesAsync(ct);

        await Result<object>.Success(new { isActive = usuario.IsActive })
            .ToResult().ExecuteAsync(HttpContext);
    }
}