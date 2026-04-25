using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using FluentValidation;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Infrastructure.Services;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Documents.Plantillas;

public record UploadPlantillaRequest
{
    public TipoPlantilla TipoDocumento { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

public class UploadPlantillaValidator : Validator<UploadPlantillaRequest>
{
    public UploadPlantillaValidator(Microsoft.Extensions.Options.IOptions<BACKSGEDI.Configuration.StorageOptions> options)
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => FileValidationHelper.IsValidPdfOrWord(f, options.Value.MaxFileSizeInBytes))
            .WithMessage($"La plantilla debe ser PDF o Word y menor a {options.Value.MaxFileSizeInBytes / 1024 / 1024}MB.");
    }
}

public class UploadPlantilla : Endpoint<UploadPlantillaRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;

    public UploadPlantilla(ApplicationDbContext db, IStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public override void Configure()
    {
        Post("/api/plantillas");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadPlantillaRequest req, CancellationToken ct)
    {
        var requesterId = User.GetUserId();

        var existing = await _db.PlantillasDocumentos
            .Where(p => p.TipoDocumento == req.TipoDocumento && p.EsVigente)
            .ToListAsync(ct);

        foreach (var p in existing) p.EsVigente = false;

        var path = await _storageService.UploadPlantillaAsync(req.File, req.TipoDocumento.ToString(), ct);

        var newPlantilla = new PlantillaDocumento
        {
            TipoDocumento = req.TipoDocumento,
            Nombre = req.Nombre,
            RutaArchivo = path,
            SubidaPorUsuarioId = requesterId,
            EsVigente = true
        };

        await _db.PlantillasDocumentos.AddAsync(newPlantilla, ct);
        await _db.SaveChangesAsync(ct);

        await Result<object>.Success(new { id = newPlantilla.Id, nombre = newPlantilla.Nombre })
            .ToResult().ExecuteAsync(HttpContext);
    }
}

