using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Plantillas;

public record UpdatePlantillaRequest
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}

public class UpdatePlantillaValidator : Validator<UpdatePlantillaRequest>
{
    public UpdatePlantillaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es requerido.");
    }
}

public class UpdatePlantilla : Endpoint<UpdatePlantillaRequest>
{
    private readonly ApplicationDbContext _db;

    public UpdatePlantilla(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/api/plantillas/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(UpdatePlantillaRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");
        if (id != req.Id)
        {
            await Result.Failure(Error.Validation("Plantilla.IdMismatch", "El ID no coincide."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var plantilla = await _db.PlantillasDocumentos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (plantilla == null)
        {
            await Result.Failure(Error.NotFound("Plantilla.NotFound", "Plantilla no encontrada."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        plantilla.Nombre = req.Nombre;
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
