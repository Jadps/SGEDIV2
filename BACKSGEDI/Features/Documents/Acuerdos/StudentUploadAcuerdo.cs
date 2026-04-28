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

public record StudentUploadAcuerdoRequest
{
    public TipoAcuerdo TipoAcuerdo { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class StudentUploadAcuerdoValidator : Validator<StudentUploadAcuerdoRequest>
{
    public StudentUploadAcuerdoValidator(Microsoft.Extensions.Options.IOptions<BACKSGEDI.Configuration.StorageOptions> options)
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => FileValidationHelper.IsValidPdf(f, options.Value.MaxFileSizeInBytes))
            .WithMessage($"El archivo debe ser un PDF y menor a {options.Value.MaxFileSizeInBytes / 1024 / 1024}MB.");
    }
}

public class StudentUploadAcuerdo : Endpoint<StudentUploadAcuerdoRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;
    private readonly BACKSGEDI.Domain.Interfaces.IFechasLimiteService _fechasLimiteService;

    public StudentUploadAcuerdo(ApplicationDbContext db, IStorageService storageService, BACKSGEDI.Domain.Interfaces.IFechasLimiteService fechasLimiteService)
    {
        _db = db;
        _storageService = storageService;
        _fechasLimiteService = fechasLimiteService;
    }

    public override void Configure()
    {
        Post("/api/alumnos/me/documentos/acuerdos");
        Roles(SystemRoles.Alumno);
        AllowFileUploads();
    }

    public override async Task HandleAsync(StudentUploadAcuerdoRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var alumno = await _db.Alumnos.FirstOrDefaultAsync(a => a.UsuarioId == userId, ct);
        
        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "No se encontró el perfil de alumno."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (!DocumentoPermissions.CanUpload(req.TipoAcuerdo, new List<string> { SystemRoles.Alumno }))
        {
            await Result.Failure(Error.Forbidden("Doc.Forbidden", "No tienes permiso para subir este tipo de anexo."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var semestreActual = SemestreHelper.GetSemestreActual();

        var currentVersion = await _db.DocumentosAcuerdos
            .Where(a => a.AlumnoId == alumno.Id && a.TipoAcuerdo == req.TipoAcuerdo && a.Semestre == semestreActual && a.EsVersionActual)
            .FirstOrDefaultAsync(ct);

        int nextVersion = 1;
        DateTime fechaLimite; 

        if (currentVersion != null)
        {
            if (currentVersion.Estado == EstadoDocumento.Aprobado)
            {
                await Result.Failure(Error.Conflict("Doc.AlreadyApproved", "Este documento ya ha sido aprobado y no puede ser modificado."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }

            currentVersion.EsVersionActual = false;
            nextVersion = currentVersion.Version + 1;
            fechaLimite = currentVersion.FechaLimite; 
        }
        else 
        {
            fechaLimite = await _fechasLimiteService.GetFechaLimiteAsync(req.TipoAcuerdo, alumno.CarreraId, semestreActual, ct);
        }

        if (DateTime.UtcNow > fechaLimite)
        {
            await Result.Failure(Error.Conflict("Doc.DeadlinePassed", "La fecha límite para subir este documento ha pasado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var path = await _storageService.UploadFileAsync(req.File, alumno.Id.ToString(), req.TipoAcuerdo.ToString(), semestreActual, ct);

        var newDoc = new DocumentoAcuerdo
        {
            AlumnoId = alumno.Id,
            TipoAcuerdo = req.TipoAcuerdo,
            Semestre = semestreActual,
            RutaArchivo = path,
            Version = nextVersion,
            EsVersionActual = true,
            Estado = EstadoDocumento.PendienteRevision,
            SubidoPorUsuarioId = userId,
            FechaSubida = DateTime.UtcNow,
            FechaLimite = fechaLimite
        };

        await _db.DocumentosAcuerdos.AddAsync(newDoc, ct);
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
