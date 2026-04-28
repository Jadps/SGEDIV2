using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Infrastructure.Services;

namespace BACKSGEDI.Features.Documents.Plantillas;

public record UpdatePlantillaRequest
{
    public int Id { get; init; }
    public IFormFile File { get; init; } = null!;
}

public class UpdatePlantillaValidator : Validator<UpdatePlantillaRequest>
{
    public UpdatePlantillaValidator(Microsoft.Extensions.Options.IOptions<BACKSGEDI.Configuration.StorageOptions> options)
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => FileValidationHelper.IsValidWord(f, options.Value.MaxFileSizeInBytes))
            .WithMessage($"La plantilla debe ser un documento de Word (.doc o .docx) y menor a {options.Value.MaxFileSizeInBytes / 1024 / 1024}MB.");
    }
}

public class UpdatePlantilla : Endpoint<UpdatePlantillaRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;

    public UpdatePlantilla(ApplicationDbContext db, IStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public override void Configure()
    {
        Patch("/api/plantillas/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
        AllowFileUploads();
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

        var path = await _storageService.UploadPlantillaAsync(req.File, plantilla.TipoDocumento.ToString(), ct);
        
        plantilla.RutaArchivo = path;
        plantilla.FechaSubida = DateTime.UtcNow;
        plantilla.SubidaPorUsuarioId = User.GetUserId();

        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
