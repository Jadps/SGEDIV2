using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Common;

namespace BACKSGEDI.Features.Alumnos.Get;

public record MyAlumnoProfileDto
{
    public Guid Id { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
}

public class GetMyAlumnoProfile : EndpointWithoutRequest<MyAlumnoProfileDto>
{
    private readonly ApplicationDbContext _db;

    public GetMyAlumnoProfile(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/alumnos/me");
        Roles(SystemRoles.Alumno);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var alumno = await _db.Alumnos
            .AsNoTracking()
            .Include(a => a.Carrera)
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.UsuarioId == userId, ct);

        if (alumno == null)
        {
            await Result<MyAlumnoProfileDto>.Failure(Error.NotFound("Alumno.NotFound", "No se encontró el perfil del alumno."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Result<MyAlumnoProfileDto>.Success(new MyAlumnoProfileDto
        {
            Id = alumno.Id,
            Matricula = alumno.Matricula,
            Carrera = alumno.Carrera.Nombre,
            NombreCompleto = $"{alumno.Usuario.Name}"
        }).ToResult().ExecuteAsync(HttpContext);
    }
}
