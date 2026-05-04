using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Evaluaciones;

public record SubmitEvaluacionRequest
{
    public Guid AlumnoId { get; init; }
    public int Calificacion { get; init; }
    public string Observaciones { get; init; } = string.Empty;
}

public class SubmitEvaluacionValidator : Validator<SubmitEvaluacionRequest>
{
    public SubmitEvaluacionValidator()
    {
        RuleFor(x => x.Calificacion)
            .InclusiveBetween(70, 100)
            .WithMessage("La calificación debe estar entre 70 y 100.");

        RuleFor(x => x.Observaciones)
            .NotEmpty()
            .WithMessage("La descripción del desempeño es obligatoria.")
            .MaximumLength(1000)
            .WithMessage("La descripción no puede exceder los 1000 caracteres.");
    }
}

public class SubmitEvaluacion : Endpoint<SubmitEvaluacionRequest>
{
    private readonly ApplicationDbContext _db;

    public SubmitEvaluacion(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/evaluaciones/{alumnoId}");
        Roles(SystemRoles.AsesorInterno, SystemRoles.AsesorExterno, SystemRoles.Admin);
    }

    public override async Task HandleAsync(SubmitEvaluacionRequest req, CancellationToken ct)
    {
        var requesterId = User.GetUserId();
        var roles = User.GetRoles();
        var isAdmin = roles.Contains(SystemRoles.Admin);

        var currentMonth = DateTime.UtcNow.Month;
        var isPeriod1 = currentMonth == DocumentConstants.EvaluationPeriod1_Month;
        var isPeriod2 = currentMonth == DocumentConstants.EvaluationPeriod2_Month;

        if (!isAdmin && !isPeriod1 && !isPeriod2)
        {
            await Result.Failure(Error.Forbidden("Eval.NotInPeriod", "Solo se pueden registrar evaluaciones durante junio y noviembre."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var alumno = await _db.Alumnos
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Id == req.AlumnoId, ct);

        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "El alumno no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (!isAdmin)
        {
            var isMyIntern = roles.Contains(SystemRoles.AsesorInterno) && 
                            await _db.AsesoresInternos.AnyAsync(ai => ai.UsuarioId == requesterId && ai.Id == alumno.AsesorInternoId, ct);
            var isMyExtern = roles.Contains(SystemRoles.AsesorExterno) && 
                            await _db.AsesoresExternos.AnyAsync(ae => ae.UsuarioId == requesterId && ae.Id == alumno.AsesorExternoId, ct);

            if (!isMyIntern && !isMyExtern)
            {
                await Result.Failure(Error.Forbidden("Eval.Forbidden", "No tienes permiso para evaluar a este alumno."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }

        var semestreActual = SemestreHelper.GetSemestreActual();

        var exists = await _db.EvaluacionesDesempeño.AnyAsync(e => 
            e.AlumnoId == req.AlumnoId && 
            e.EvaluadorId == requesterId && 
            e.Semestre == semestreActual, ct);

        if (exists && !isAdmin)
        {
            await Result.Failure(Error.Conflict("Eval.Exists", "Ya has registrado una evaluación para este alumno en el periodo actual."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var eval = new EvaluacionDesempeño
        {
            AlumnoId = req.AlumnoId,
            EvaluadorId = requesterId,
            Calificacion = req.Calificacion,
            Observaciones = req.Observaciones,
            Semestre = semestreActual,
            FechaEvaluacion = DateTime.UtcNow
        };

        await _db.EvaluacionesDesempeño.AddAsync(eval, ct);
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
