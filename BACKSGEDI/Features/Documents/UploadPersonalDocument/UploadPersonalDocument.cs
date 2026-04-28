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
using System.Security.Claims;

namespace BACKSGEDI.Features.Documents.UploadPersonalDocument;

public record UploadPersonalDocumentRequest
{
    public Guid AlumnoId { get; set; }
    public TipoDocumentoAlumno TipoDocumento { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UploadPersonalDocumentValidator : Validator<UploadPersonalDocumentRequest>
{
    public UploadPersonalDocumentValidator(Microsoft.Extensions.Options.IOptions<BACKSGEDI.Configuration.StorageOptions> options)
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => FileValidationHelper.IsValidPdf(f, options.Value.MaxFileSizeInBytes))
            .WithMessage($"El documento debe ser un PDF y menor a {options.Value.MaxFileSizeInBytes / 1024 / 1024}MB.");
    }
}

public class UploadPersonalDocument : Endpoint<UploadPersonalDocumentRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;

    public UploadPersonalDocument(ApplicationDbContext db, IStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public override void Configure()
    {
        Post("/api/alumnos/{alumnoId}/documentos");
        Roles(SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadPersonalDocumentRequest req, CancellationToken ct)
    {
        var requesterId = User.GetUserId();
        var roles = User.GetRoles();
        var isAdminOrCoord = roles.Contains(SystemRoles.Admin) || roles.Contains(SystemRoles.Coordinador);
        
        var isMe = await _db.Alumnos.AnyAsync(a => a.Id == req.AlumnoId && a.UsuarioId == requesterId, ct);
        if (!isMe && !isAdminOrCoord)
        {
            await Result.Failure(Error.Forbidden("Doc.Forbidden", "No puedes subir documentos de otro alumno."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var semestreActual = SemestreHelper.GetSemestreActual();

        var currentVersion = await _db.DocumentosAlumnos
            .Where(d => d.AlumnoId == req.AlumnoId && d.TipoDocumento == req.TipoDocumento && d.Semestre == semestreActual && d.EsVersionActual)
            .FirstOrDefaultAsync(ct);

        int nextVersion = 1;
        if (currentVersion != null)
        {
            currentVersion.EsVersionActual = false;
            nextVersion = currentVersion.Version + 1;
        }

        var path = await _storageService.UploadFileAsync(req.File, req.AlumnoId.ToString(), req.TipoDocumento.ToString(), semestreActual, ct);

        var newDoc = new DocumentoAlumno
        {
            AlumnoId = req.AlumnoId,
            TipoDocumento = req.TipoDocumento,
            Semestre = semestreActual,
            RutaArchivo = path,
            Version = nextVersion,
            EsVersionActual = true,
            Estado = isAdminOrCoord ? EstadoDocumento.Aprobado : EstadoDocumento.PendienteRevision,
            SubidoPorUsuarioId = requesterId
        };

        await _db.DocumentosAlumnos.AddAsync(newDoc, ct);
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}

