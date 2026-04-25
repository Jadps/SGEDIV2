using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using BACKSGEDI.Infrastructure.Services;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BACKSGEDI.Features.Documents.Acuerdos;

public record UploadAcuerdoRequest
{
    public Guid AcuerdoId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UploadAcuerdo : Endpoint<UploadAcuerdoRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;

    public UploadAcuerdo(ApplicationDbContext db, IStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public override void Configure()
    {
        Post("/api/acuerdos/{acuerdoId}/upload");
        Roles(SystemRoles.Alumno, SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno, 
              SystemRoles.Admin, SystemRoles.Coordinador);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadAcuerdoRequest req, CancellationToken ct)
    {
        var acuerdo = await _db.DocumentosAcuerdos.FirstOrDefaultAsync(a => a.Id == req.AcuerdoId, ct);
        if (acuerdo == null) {
            await Result.Failure(Error.NotFound("Doc.NotFound", "El documento no existe."))
    .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var roles = User.GetRoles();
        
        if (!DocumentoPermissions.CanUpload(acuerdo.TipoAcuerdo, roles))
        {
            await Result.Failure(Error.Forbidden("Doc.Forbidden", "No puedes subir documentos de otro rol."))
    .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var requesterId = User.GetUserId();
        var semestreActual = SemestreHelper.GetSemestreActual();
        
        if (acuerdo.FechaSubida != null)
        {
            acuerdo.EsVersionActual = false;
            var maxVersion = await _db.DocumentosAcuerdos
      .Where(a => a.AlumnoId == acuerdo.AlumnoId && a.TipoAcuerdo == acuerdo.TipoAcuerdo && a.Semestre == acuerdo.Semestre)
      .MaxAsync(a => a.Version, ct);
            var nexVersion = new DocumentoAcuerdo
            {
                AlumnoId = acuerdo.AlumnoId,
                ProfesorId = acuerdo.ProfesorId,
                AsesorInternoId = acuerdo.AsesorInternoId,
                AsesorExternoId = acuerdo.AsesorExternoId,
                TipoAcuerdo = acuerdo.TipoAcuerdo,
                Semestre = acuerdo.Semestre,
                FechaLimite = acuerdo.FechaLimite,
                Version = maxVersion + 1,
                EsVersionActual = true,
                Estado = EstadoDocumento.PendienteRevision,
                SubidoPorUsuarioId = requesterId
            };

            var path = await _storageService.UploadFileAsync(req.File, acuerdo.AlumnoId.ToString(), acuerdo.TipoAcuerdo.ToString(), acuerdo.Semestre, ct);
            nexVersion.RutaArchivo = path;
            nexVersion.FechaSubida = DateTime.UtcNow;

            await _db.DocumentosAcuerdos.AddAsync(nexVersion, ct);
        }
        else
        {
            var path = await _storageService.UploadFileAsync(req.File, acuerdo.AlumnoId.ToString(), acuerdo.TipoAcuerdo.ToString(), acuerdo.Semestre, ct);
            acuerdo.RutaArchivo = path;
            acuerdo.FechaSubida = DateTime.UtcNow;
            acuerdo.SubidoPorUsuarioId = requesterId;
            acuerdo.Estado = EstadoDocumento.PendienteRevision;
        }

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}

