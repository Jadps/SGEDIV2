using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace BACKSGEDI.Features.Documents.Download;

public class GetDocumentById : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    private readonly string _basePath;

    public GetDocumentById(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _basePath = configuration["Storage:BasePath"] ?? "Uploads";
    }

    public override void Configure()
    {
        Get("/api/documentos/{id}/download");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idStr = Route<string>("id");
        if (!Guid.TryParse(idStr, out var docId))
        {
            await Result.Failure(Error.Validation("Documento.InvalidId", "El ID del documento no es válido."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        string rutaArchivo = "";
        string fileNameBase = "";

        var docAcuerdo = await _db.DocumentosAcuerdos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == docId, ct);
        Guid alumnoIdDoc;

        if (docAcuerdo != null)
        {
            rutaArchivo = docAcuerdo.RutaArchivo;
            fileNameBase = $"{docAcuerdo.TipoAcuerdo}_{docAcuerdo.AlumnoId}";
            alumnoIdDoc = docAcuerdo.AlumnoId;
        }
        else
        {
            var docAlumno = await _db.DocumentosAlumnos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == docId, ct);
            if (docAlumno != null)
            {
                rutaArchivo = docAlumno.RutaArchivo;
                fileNameBase = $"{docAlumno.TipoDocumento}_{docAlumno.AlumnoId}";
                alumnoIdDoc = docAlumno.AlumnoId;
            }
            else
            {
                await Result.Failure(Error.NotFound("Documento.NotFound", "El documento no existe."))
                    .ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }

        var userId = User.GetUserId();
        var roles = User.GetRoles();
        
        var tienePermiso = await _db.Alumnos
            .IgnoreQueryFilters()
            .ApplySecurityFilter(userId, roles, _db)
            .AnyAsync(a => a.Id == alumnoIdDoc && !a.IsDeleted, ct);

        if (!tienePermiso)
        {
            await Result.Failure(Error.Forbidden("Documento.Forbidden", "No tienes permiso para acceder a este documento."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (string.IsNullOrEmpty(rutaArchivo))
        {
            await Result.Failure(Error.NotFound("Documento.NotFound", "El documento no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), _basePath, rutaArchivo);

        if (!File.Exists(rutaAbsoluta))
        {
            await Result.Failure(Error.NotFound("Documento.FileNotFound", "El archivo físico no se encontró en el servidor."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var ext = Path.GetExtension(rutaArchivo).ToLowerInvariant();
        var contentType = ext switch
        {
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/pdf"
        };

        await Results.File(rutaAbsoluta, contentType, $"{fileNameBase}{ext}").ExecuteAsync(HttpContext);
    }
}
