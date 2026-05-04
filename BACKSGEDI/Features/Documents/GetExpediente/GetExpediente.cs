using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Documents.GetExpediente;

public record ExpedienteItemDto
{
    public Guid? DocumentoId { get; init; }
    public int TipoId { get; init; }
    public string Label { get; init; } = string.Empty;
    public string? Semestre { get; init; }
    public int Estado { get; init; }
    public int Version { get; init; }
    public DateTime? FechaLimite { get; init; }
    public bool PuedeSubir { get; init; }
    public bool EsAcuerdo { get; init; }
    public FileDetailsDto? Archivo { get; init; }
    public Guid? ProfesorId { get; init; }
    public Guid? MateriaId { get; init; }
}

public record FileDetailsDto
{
    public Guid Id { get; init; }
    public DateTime FechaSubida { get; init; }
}

public record GetExpedienteRequest
{
    [BindFrom("alumnoId")]
    public string AlumnoIdRaw { get; init; } = string.Empty;
    public string? Semestre { get; init; }
}

public class GetExpediente : Endpoint<GetExpedienteRequest, List<ExpedienteItemDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly IFechasLimiteService _fechasLimiteService;

    public GetExpediente(ApplicationDbContext db, IFechasLimiteService fechasLimiteService)
    {
        _db = db;
        _fechasLimiteService = fechasLimiteService;
    }

    public override void Configure()
    {
        Get("/api/alumnos/{alumnoId}/expediente");
        Roles(SystemRoles.Alumno, SystemRoles.Coordinador, SystemRoles.Admin,
              SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno);
    }

    public override async Task HandleAsync(GetExpedienteRequest req, CancellationToken ct)
    {
        var semester = req.Semestre ?? SemestreHelper.GetSemestreActual();
        var roles    = User.GetRoles();
        var userId   = User.GetUserId();

        var myProfesorId = await _db.Profesores
            .AsNoTracking()
            .Where(p => p.UsuarioId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct);

        Guid alumnoId;
        if (req.AlumnoIdRaw.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            var myAlumno = await _db.Alumnos
                .AsNoTracking()
                .Where(a => a.UsuarioId == userId)
                .Select(a => (Guid?)a.Id)
                .FirstOrDefaultAsync(ct);

            if (myAlumno is null)
            {
                await Result.Failure(Error.NotFound("Alumno.NotFound", "Perfil de alumno no encontrado.")).ToResult().ExecuteAsync(HttpContext);
                return;
            }
            alumnoId = myAlumno.Value;
        }
        else if (!Guid.TryParse(req.AlumnoIdRaw, out alumnoId))
        {
            await Result.Failure(Error.Validation("Alumno.InvalidId", "Identificador de alumno inválido.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var alumno = await _db.Alumnos
            .AsNoTracking()
            .Where(a => a.Id == alumnoId)
            .Select(a => new { a.CarreraId })
            .FirstOrDefaultAsync(ct);

        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "Perfil de alumno no encontrado.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var agreements = await _db.DocumentosAcuerdos
            .Include(d => d.Materia)
            .AsNoTracking()
            .Where(d => d.AlumnoId == alumnoId && d.Semestre == semester && d.EsVersionActual)
            .Select(d => new ExpedienteItemDto
            {
                DocumentoId = d.Id,
                TipoId = (int)d.TipoAcuerdo,
                Label = d.MateriaId != null ? $"{d.TipoAcuerdo.ToString().Replace("Anexo", "Anexo ")} - {d.Materia!.Nombre}" : d.TipoAcuerdo.ToString().Replace("Anexo", "Anexo "),
                Semestre = d.Semestre,
                Estado = !string.IsNullOrEmpty(d.RutaArchivo) ? (int)d.Estado : -1,
                Version = d.Version,
                FechaLimite = d.FechaLimite,
                PuedeSubir = false,
                EsAcuerdo = true,
                ProfesorId = d.ProfesorId,
                MateriaId = d.MateriaId,
                Archivo = !string.IsNullOrEmpty(d.RutaArchivo) ? new FileDetailsDto { Id = d.Id, FechaSubida = d.FechaSubida ?? DateTime.MinValue } : null
            })
            .ToListAsync(ct);

        var personalDocs = await _db.DocumentosAlumnos
            .AsNoTracking()
            .Where(d => d.AlumnoId == alumnoId && d.Semestre == semester && d.EsVersionActual)
            .Select(d => new ExpedienteItemDto
            {
                DocumentoId = d.Id,
                TipoId = (int)d.TipoDocumento,
                Label = d.TipoDocumento.ToString(),
                Semestre = d.Semestre,
                Estado = (int)d.Estado,
                Version = d.Version,
                EsAcuerdo = false,
                Archivo = new FileDetailsDto { Id = d.Id, FechaSubida = d.FechaSubida }
            })
            .ToListAsync(ct);

        var carga = await _db.CargasAcademicas
            .AsNoTracking()
            .Include(ca => ca.Materia)
            .Where(ca => ca.AlumnoId == alumnoId && ca.Semestre == semester)
            .ToListAsync(ct);

        var resultList = new List<ExpedienteItemDto>();
        var configs = await _db.ConfiguracionesFechasLimites
            .AsNoTracking()
            .Where(f => f.CarreraId == alumno.CarreraId && f.Semestre == semester)
            .ToListAsync(ct);

        foreach (TipoAcuerdo tipo in Enum.GetValues(typeof(TipoAcuerdo)))
        {
            var config = configs.FirstOrDefault(f => f.TipoAcuerdo == tipo);
            var matched = agreements.Where(a => a.TipoId == (int)tipo).ToList();

            if (DocumentoPermissions.IsMultiInstance(tipo))
            {
                foreach (var m in carga)
                {
                    if (roles.Contains(SystemRoles.Profesor) && !roles.Contains(SystemRoles.Admin) && !roles.Contains(SystemRoles.Coordinador))
                    {
                        if (m.ProfesorId != myProfesorId) continue;
                    }

                    var doc = matched.FirstOrDefault(a => a.MateriaId == m.MateriaId);
                    if (doc != null)
                    {
                        bool puedeSubir = DocumentoPermissions.CanUpload(tipo, roles) || (doc.ProfesorId == myProfesorId);
                        resultList.Add(doc with { PuedeSubir = puedeSubir });
                    }
                    else
                    {
                        resultList.Add(new ExpedienteItemDto
                        {
                            TipoId = (int)tipo,
                            Label = $"{tipo.ToString().Replace("Anexo", "Anexo ")} - {m.Materia?.Nombre}",
                            Semestre = semester,
                            Estado = -1,
                            FechaLimite = config?.FechaLimite ?? _fechasLimiteService.CalculateDefault(tipo, semester),
                            PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles) || (m.ProfesorId == myProfesorId),
                            EsAcuerdo = true,
                            MateriaId = m.MateriaId,
                            ProfesorId = m.ProfesorId
                        });
                    }
                }
            }
            else
            {
                if (!matched.Any())
                {
                    resultList.Add(new ExpedienteItemDto
                    {
                        TipoId = (int)tipo,
                        Label = tipo.ToString().Replace("Anexo", "Anexo "),
                        Semestre = semester,
                        Estado = -1,
                        FechaLimite = config?.FechaLimite ?? _fechasLimiteService.CalculateDefault(tipo, semester),
                        PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles),
                        EsAcuerdo = true
                    });
                }
                else
                {
                    foreach (var doc in matched)
                    {
                        resultList.Add(doc with { PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles) });
                    }
                }
            }
        }

        foreach (TipoDocumentoAlumno tipo in Enum.GetValues(typeof(TipoDocumentoAlumno)))
        {
            var doc = personalDocs.FirstOrDefault(d => d.TipoId == (int)tipo);
            if (doc == null)
            {
                resultList.Add(new ExpedienteItemDto
                {
                    TipoId = (int)tipo,
                    Label = tipo.ToString(),
                    Semestre = semester,
                    Estado = -1,
                    PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles),
                    EsAcuerdo = false
                });
            }
            else
            {
                resultList.Add(doc with { PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles) });
            }
        }

        await Result<List<ExpedienteItemDto>>.Success(resultList).ToResult().ExecuteAsync(HttpContext);
    }
}
