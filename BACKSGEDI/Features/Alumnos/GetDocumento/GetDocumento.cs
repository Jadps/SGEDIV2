using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BACKSGEDI.Features.Alumnos.GetDocumento;

public class GetDocumento : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    private readonly string _basePath;

    public GetDocumento(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _basePath = configuration["Storage:BasePath"] ?? "Uploads";
    }

    public override void Configure()
    {
        Get("/api/alumnos/{alumnoId}/documentos/{tipoDocumento}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var requesterIdClaim = User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(requesterIdClaim) || !Guid.TryParse(requesterIdClaim, out var requesterId))
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "No autorizado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var alumnoIdStr = Route<string>("alumnoId");
        if (!Guid.TryParse(alumnoIdStr, out var alumnoId))
        {
            await Result.Failure(Error.Validation("Documento.InvalidId", "El ID del alumno no es válido."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var tipoStr = Route<string>("tipoDocumento");
        if (!Enum.TryParse<TipoDocumentoAlumno>(tipoStr, ignoreCase: true, out var tipoDocumento))
        {
            await Result.Failure(Error.Validation("Documento.InvalidType",
                $"Tipo de documento inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoDocumentoAlumno>())}"))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var isAdmin       = roles.Any(r => string.Equals(r, SystemRoles.Admin,            StringComparison.OrdinalIgnoreCase));
        var isCoordinador = roles.Any(r => string.Equals(r, SystemRoles.Coordinador,      StringComparison.OrdinalIgnoreCase));
        var isJefe        = roles.Any(r => string.Equals(r, SystemRoles.JefeDepartamento, StringComparison.OrdinalIgnoreCase));
        var isAlumno      = roles.Any(r => string.Equals(r, SystemRoles.Alumno,           StringComparison.OrdinalIgnoreCase));

        bool tienePermiso;

        if (isAdmin)
        {
            tienePermiso = true;
        }
        else if (isAlumno)
        {
            var alumnoDelUsuario = await _db.Alumnos
                .AsNoTracking()
                .AnyAsync(a => a.Id == alumnoId && a.UsuarioId == requesterId, ct);
            tienePermiso = alumnoDelUsuario;
        }
        else if (isCoordinador)
        {
            var carreraCoordinador = await _db.Coordinadores
                .AsNoTracking()
                .Where(c => c.UsuarioId == requesterId)
                .Select(c => c.CarreraId)
                .FirstOrDefaultAsync(ct);

            tienePermiso = carreraCoordinador != 0 &&
                           await _db.Alumnos.AsNoTracking()
                               .AnyAsync(a => a.Id == alumnoId && a.CarreraId == carreraCoordinador, ct);
        }
        else if (isJefe)
        {
            var carreraJefe = await _db.JefesDepartamento
                .AsNoTracking()
                .Where(j => j.UsuarioId == requesterId)
                .Select(j => j.CarreraId)
                .FirstOrDefaultAsync(ct);

            tienePermiso = carreraJefe != 0 &&
                           await _db.Alumnos.AsNoTracking()
                               .AnyAsync(a => a.Id == alumnoId && a.CarreraId == carreraJefe, ct);
        }
        else
        {
            tienePermiso = false;
        }

        if (!tienePermiso)
        {
            await Result.Failure(Error.Forbidden("Documento.Forbidden",
                "No tienes permiso para acceder a este documento."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

var documento = await _db.DocumentosAlumnos
    .AsNoTracking()
    .Where(d => d.AlumnoId == alumnoId && d.TipoDocumento == tipoDocumento && d.EsVersionActual)
    .FirstOrDefaultAsync(ct);

        if (documento is null)
        {
            await Result.Failure(Error.NotFound("Documento.NotFound",
                "El documento solicitado no existe en la base de datos."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), _basePath, documento.RutaArchivo);

        if (!File.Exists(rutaAbsoluta))
        {
            await Result.Failure(Error.NotFound("Documento.FileNotFound",
                "El archivo físico no se encontró en el servidor."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Results.File(rutaAbsoluta, "application/pdf", $"{tipoDocumento}_{alumnoId}.pdf")
                     .ExecuteAsync(HttpContext);
    }
}
