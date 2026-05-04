using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Domain.Common;


namespace BACKSGEDI.Features.Alumnos;

public record ActivateStudentRequest
{
    public Guid AlumnoId { get; init; }
    public Guid EmpresaId { get; init; }
    public Guid AsesorInternoId { get; init; }
    public Guid AsesorExternoId { get; init; }
}

public class ActivateStudentAccount : Endpoint<ActivateStudentRequest>
{
    private readonly ApplicationDbContext _db;

    public ActivateStudentAccount(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/alumnos/{alumnoId}/activate");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(ActivateStudentRequest req, CancellationToken ct)
    {
        var alumno = await _db.Alumnos
            .Include(a => a.Usuario)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == req.AlumnoId, ct);

        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "El alumno no existe.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var empresaExists = await _db.Empresas.AnyAsync(e => e.Id == req.EmpresaId, ct);
        var asInternoExists = await _db.AsesoresInternos.AnyAsync(ai => ai.Id == req.AsesorInternoId, ct);
        var asExternoExists = await _db.AsesoresExternos.AnyAsync(ae => ae.Id == req.AsesorExternoId, ct);

        if (!empresaExists || !asInternoExists || !asExternoExists)
        {
            await Result.Failure(Error.NotFound("Validation.Failed", "Empresa o Asesores no encontrados.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        alumno.EmpresaId = req.EmpresaId;
        alumno.AsesorInternoId = req.AsesorInternoId;
        alumno.AsesorExternoId = req.AsesorExternoId;
        
        if (alumno.Usuario != null)
        {
            alumno.Usuario.Status = (int)EntityStatus.Activo;
        }
        
        alumno.Status = (int)EntityStatus.Activo;

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
