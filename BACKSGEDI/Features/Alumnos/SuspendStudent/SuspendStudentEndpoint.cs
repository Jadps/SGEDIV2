using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Alumnos.SuspendStudent;

public class SuspendStudentEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    public SuspendStudentEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/api/alumnos/{id}/suspend");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var userId = User.GetUserId();
        var roles = User.GetRoles();

        var alumno = await _db.Alumnos
            .Include(a => a.Usuario)
            .ApplySecurityFilter(userId, roles, _db)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "El alumno no existe o no tienes permiso."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (alumno.Status != (int)EntityStatus.Activo)
        {
            await Result.Failure(Error.Conflict("Alumno.NotActive", "Solo se puede suspender un alumno activo."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        alumno.Status = (int)EntityStatus.SinActivar;
        if (alumno.Usuario != null)
            alumno.Usuario.Status = (int)EntityStatus.SinActivar;

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
