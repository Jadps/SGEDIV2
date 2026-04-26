using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Alumnos.Get;

public class GetStudentSemesters : EndpointWithoutRequest<List<string>>
{
    private readonly ApplicationDbContext _db;

    public GetStudentSemesters(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/alumnos/{alumnoId}/semestres");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var alumnoId = Route<Guid>("alumnoId");

        var semestresAlumnos = await _db.DocumentosAlumnos
            .Where(d => d.AlumnoId == alumnoId)
            .Select(d => d.Semestre)
            .Distinct()
            .ToListAsync(ct);

        var semestresAcuerdos = await _db.DocumentosAcuerdos
            .Where(d => d.AlumnoId == alumnoId)
            .Select(d => d.Semestre)
            .Distinct()
            .ToListAsync(ct);

        var result = semestresAlumnos.Union(semestresAcuerdos)
            .OrderByDescending(s => s)
            .ToList();

        if (!result.Any())
        {
            result.Add(SemestreHelper.GetSemestreActual());
        }

        await Result<List<string>>.Success(result)
            .ToResult().ExecuteAsync(HttpContext);
    }
}
