using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Infrastructure.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Documents.Acuerdos;

public record UploadAdministrativeAcuerdoRequest
{
    public Guid? AcuerdoId { get; set; }
    public Guid? AlumnoId { get; set; }
    public TipoAcuerdo? TipoAcuerdo { get; set; }
    public Guid? ProfesorId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UploadAdministrativeAcuerdoValidator : Validator<UploadAdministrativeAcuerdoRequest>
{
    public UploadAdministrativeAcuerdoValidator(Microsoft.Extensions.Options.IOptions<BACKSGEDI.Configuration.StorageOptions> options)
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => FileValidationHelper.IsValidPdfOrWord(f, options.Value.MaxFileSizeInBytes))
            .WithMessage($"El archivo debe ser PDF o Word y menor a {options.Value.MaxFileSizeInBytes / 1024 / 1024}MB.");

        RuleFor(x => x)
            .Must(x => x.AcuerdoId.HasValue || (x.AlumnoId.HasValue && x.TipoAcuerdo.HasValue))
            .WithMessage("Se requiere el ID del acuerdo o el AlumnoId y TipoAcuerdo.");
    }
}

public class UploadAdministrativeAcuerdo : Endpoint<UploadAdministrativeAcuerdoRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;
    private readonly BACKSGEDI.Domain.Interfaces.IFechasLimiteService _fechasLimiteService;

    public UploadAdministrativeAcuerdo(ApplicationDbContext db, IStorageService storageService, BACKSGEDI.Domain.Interfaces.IFechasLimiteService fechasLimiteService)
    {
        _db = db;
        _storageService = storageService;
        _fechasLimiteService = fechasLimiteService;
    }

    public override void Configure()
    {
        Post("/api/acuerdos/administrative-upload");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadAdministrativeAcuerdoRequest req, CancellationToken ct)
    {
        var requesterId = User.GetUserId();
        var semestreActual = SemestreHelper.GetSemestreActual();
        DocumentoAcuerdo? acuerdo = null;

        if (req.AcuerdoId.HasValue)
        {
            acuerdo = await _db.DocumentosAcuerdos.FirstOrDefaultAsync(a => a.Id == req.AcuerdoId, ct);
        }
        else
        {
            acuerdo = await _db.DocumentosAcuerdos
                .FirstOrDefaultAsync(a => a.AlumnoId == req.AlumnoId 
                    && a.TipoAcuerdo == req.TipoAcuerdo 
                    && a.Semestre == semestreActual 
                    && a.EsVersionActual 
                    && (!req.ProfesorId.HasValue || a.ProfesorId == req.ProfesorId), ct);
        }

        if (acuerdo == null)
        {
            if (!req.AlumnoId.HasValue || !req.TipoAcuerdo.HasValue)
            {
                await Result.Failure(Error.Failure("Doc.MissingData", "Faltan datos para crear el documento."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }

            var alumno = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == req.AlumnoId, ct);
            if (alumno == null)
            {
                await Result.Failure(Error.NotFound("Alumno.NotFound", "El alumno no existe."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }

            var fechaLimite = await _fechasLimiteService.GetFechaLimiteAsync(req.TipoAcuerdo.Value, alumno.CarreraId, semestreActual, ct);
            var path = await _storageService.UploadFileAsync(req.File, req.AlumnoId.ToString()!, req.TipoAcuerdo.ToString()!, semestreActual, ct);

            acuerdo = new DocumentoAcuerdo
            {
                AlumnoId = req.AlumnoId.Value,
                TipoAcuerdo = req.TipoAcuerdo.Value,
                ProfesorId = req.ProfesorId,
                Semestre = semestreActual,
                RutaArchivo = path,
                Version = 1,
                EsVersionActual = true,
                Estado = EstadoDocumento.Aprobado,
                SubidoPorUsuarioId = requesterId,
                FechaSubida = DateTime.UtcNow,
                FechaLimite = fechaLimite
            };

            await _db.DocumentosAcuerdos.AddAsync(acuerdo, ct);
        }
        else
        {
            if (acuerdo.FechaSubida != null)
            {
                acuerdo.EsVersionActual = false;
                var maxVersion = await _db.DocumentosAcuerdos
                    .Where(a => a.AlumnoId == acuerdo.AlumnoId && a.TipoAcuerdo == acuerdo.TipoAcuerdo && a.Semestre == acuerdo.Semestre)
                    .MaxAsync(a => a.Version, ct);

                var nextVersion = new DocumentoAcuerdo
                {
                    AlumnoId = acuerdo.AlumnoId,
                    ProfesorId = acuerdo.ProfesorId,
                    TipoAcuerdo = acuerdo.TipoAcuerdo,
                    Semestre = acuerdo.Semestre,
                    FechaLimite = acuerdo.FechaLimite,
                    Version = maxVersion + 1,
                    EsVersionActual = true,
                    Estado = EstadoDocumento.Aprobado,
                    SubidoPorUsuarioId = requesterId,
                    FechaSubida = DateTime.UtcNow
                };

                var path = await _storageService.UploadFileAsync(req.File, acuerdo.AlumnoId.ToString(), acuerdo.TipoAcuerdo.ToString(), acuerdo.Semestre, ct);
                nextVersion.RutaArchivo = path;

                await _db.DocumentosAcuerdos.AddAsync(nextVersion, ct);
            }
            else
            {
                var path = await _storageService.UploadFileAsync(req.File, acuerdo.AlumnoId.ToString(), acuerdo.TipoAcuerdo.ToString(), acuerdo.Semestre, ct);
                acuerdo.RutaArchivo = path;
                acuerdo.FechaSubida = DateTime.UtcNow;
                acuerdo.SubidoPorUsuarioId = requesterId;
                acuerdo.Estado = EstadoDocumento.Aprobado;
            }
        }

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
