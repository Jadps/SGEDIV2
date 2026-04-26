using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Alumnos.Get;

public record StudentDeadlineDto
{
    public TipoAcuerdo TipoAcuerdo { get; init; }
    public DateTime FechaLimite { get; init; }
}

public class GetMyDeadlines : EndpointWithoutRequest<List<StudentDeadlineDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly IFechasLimiteService _fechasLimiteService;

    public GetMyDeadlines(ApplicationDbContext db, IFechasLimiteService fechasLimiteService)
    {
        _db = db;
        _fechasLimiteService = fechasLimiteService;
    }

    public override void Configure()
    {
        Get("/api/alumnos/me/fechas-limite");
        Roles(SystemRoles.Alumno);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var alumno = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.UsuarioId == userId, ct);
        
        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "Perfil no encontrado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var semestre = SemestreHelper.GetSemestreActual();

        var customFechas = await _db.ConfiguracionesFechasLimites
            .AsNoTracking()
            .Where(c => c.CarreraId == alumno.CarreraId && c.Semestre == semestre)
            .ToListAsync(ct);

        var result = new List<StudentDeadlineDto>();
        
        var tiposanexosAlumno = new[] { TipoAcuerdo.AnexoI, TipoAcuerdo.AnexoIV, TipoAcuerdo.AnexoVIII };

        foreach (var tipo in tiposanexosAlumno)
        {
            var custom = customFechas.FirstOrDefault(c => c.TipoAcuerdo == tipo);
            result.Add(new StudentDeadlineDto
            {
                TipoAcuerdo = tipo,
                FechaLimite = custom?.FechaLimite ?? _fechasLimiteService.CalculateDefault(tipo, semestre)
            });
        }

        await Result<List<StudentDeadlineDto>>.Success(result).ToResult().ExecuteAsync(HttpContext);
    }
}
