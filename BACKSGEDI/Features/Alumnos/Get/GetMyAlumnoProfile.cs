using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Common;

namespace BACKSGEDI.Features.Alumnos.Get;

public record MyAlumnoProfileDto
{
    public Guid Id { get; init; }
    public string Matricula { get; init; } = string.Empty;
    public string Carrera { get; init; } = string.Empty;
    public int CarreraId { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
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
        var dto = await _db.Alumnos
            .AsNoTracking()
            .Where(a => a.UsuarioId == userId)
            .Select(a => new MyAlumnoProfileDto
            {
                Id = a.Id,
                Matricula = a.Matricula,
                Carrera = a.Carrera.Nombre,
                CarreraId = a.CarreraId,
                NombreCompleto = a.Usuario.Name
            })
            .FirstOrDefaultAsync(ct);

        if (dto == null)
        {
            await Result<MyAlumnoProfileDto>.Failure(Error.NotFound("Alumno.NotFound", "No se encontró el perfil del alumno."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Result<MyAlumnoProfileDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}
