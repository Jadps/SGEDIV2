using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Contratos;

public record CreateContratoRequest
{
    public Guid AlumnoId { get; init; }
    public Guid MateriaId { get; init; }
    public ModalidadContrato Modalidad { get; init; }
    public string? Descripcion { get; init; }
    public List<CreateCriterioDto> Criterios { get; init; } = new();
}

public record CreateCriterioDto
{
    public TipoCriterio Tipo { get; init; }
    public string Detalle { get; init; } = string.Empty;
    public int Porcentaje { get; init; }
}

public class CreateContratoValidator : Validator<CreateContratoRequest>
{
    public CreateContratoValidator()
    {
        RuleFor(x => x.Modalidad).NotNull();
        RuleFor(x => x.Criterios).NotEmpty().WithMessage("Debe incluir al menos un criterio de evaluación.");
        RuleForEach(x => x.Criterios).ChildRules(c => {
            c.RuleFor(x => x.Detalle).NotEmpty();
            c.RuleFor(x => x.Porcentaje).InclusiveBetween(1, 100);
        });
        RuleFor(x => x.Criterios)
            .Must(c => c.Sum(x => x.Porcentaje) == 100)
            .WithMessage("La suma de los porcentajes debe ser 100%.");
    }
}

public class CreateContrato : Endpoint<CreateContratoRequest>
{
    private readonly ApplicationDbContext _db;

    public CreateContrato(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/contratos");
        Roles(SystemRoles.Profesor, SystemRoles.Admin);
    }

    public override async Task HandleAsync(CreateContratoRequest req, CancellationToken ct)
    {
        var professorId = User.GetUserId();
        
        var profesor = await _db.Profesores
            .FirstOrDefaultAsync(p => p.UsuarioId == professorId, ct);

        if (profesor == null && !User.IsInRole(SystemRoles.Admin))
        {
            await Result.Failure(Error.Forbidden("Profesor.NotFound", "No se encontró el perfil de profesor."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var exists = await _db.ContratosProfesores.AnyAsync(a => 
            a.AlumnoId == req.AlumnoId && a.MateriaId == req.MateriaId, ct);

        if (exists)
        {
            await Result.Failure(Error.Conflict("Contrato.Exists", "Ya existe un contrato para esta materia. Utilice el método de actualización."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var alumno = await _db.Alumnos
            .FirstOrDefaultAsync(a => a.Id == req.AlumnoId, ct);

        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "El alumno no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var contrato = new ContratoProfesor
        {
            AlumnoId = req.AlumnoId,
            MateriaId = req.MateriaId,
            ProfesorId = profesor?.Id ?? Guid.Empty,
            CarreraId = alumno.CarreraId,
            Modalidad = req.Modalidad,
            Descripcion = req.Descripcion,
            Estado = EstadoContrato.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            Criterios = req.Criterios.Select(c => new CriterioEvaluacion
            {
                Tipo = c.Tipo,
                Detalle = c.Detalle,
                Porcentaje = c.Porcentaje
            }).ToList()
        };

        await _db.ContratosProfesores.AddAsync(contrato, ct);
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
