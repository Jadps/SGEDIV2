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
    public string EstadoText { get; init; } = string.Empty;
    public string EstadoSeverity { get; init; } = string.Empty;
    public int Version { get; init; }
    public DateTime? FechaLimite { get; init; }
    public bool PuedeSubir { get; init; }
    public bool EsAcuerdo { get; init; }
    public FileDetailsDto? Archivo { get; init; }
}

public record FileDetailsDto
{
    public Guid Id { get; init; }
    public DateTime FechaSubida { get; init; }
}

public record GetExpedienteRequest
{
    public Guid AlumnoId { get; init; }
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
        Roles(SystemRoles.Alumno, SystemRoles.Coordinador, SystemRoles.Admin);
    }

    public override async Task HandleAsync(GetExpedienteRequest req, CancellationToken ct)
    {
        var semester = req.Semestre ?? SemestreHelper.GetSemestreActual();
        var roles = User.GetRoles();

        var alumno = await _db.Alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == req.AlumnoId, ct);

        if (alumno == null)
        {
            await Result<List<ExpedienteItemDto>>.Failure(Error.NotFound("Alumno.NotFound", "Perfil de alumno no encontrado."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var docsAcuerdos = await _db.DocumentosAcuerdos
            .AsNoTracking()
            .Where(d => d.AlumnoId == req.AlumnoId && d.Semestre == semester && d.EsVersionActual)
            .ToListAsync(ct);

        var docsAlumnos = await _db.DocumentosAlumnos
            .AsNoTracking()
            .Where(d => d.AlumnoId == req.AlumnoId && d.Semestre == semester && d.EsVersionActual)
            .ToListAsync(ct);

        var resultList = new List<ExpedienteItemDto>();

        foreach (TipoAcuerdo tipo in Enum.GetValues(typeof(TipoAcuerdo)))
        {
            var doc = docsAcuerdos.FirstOrDefault(d => d.TipoAcuerdo == tipo);
            var fechaLimite = (doc != null) 
                ? doc.FechaLimite 
                : await _fechasLimiteService.GetFechaLimiteAsync(tipo, alumno.CarreraId, semester, ct);

            resultList.Add(new ExpedienteItemDto
            {
                DocumentoId = doc?.Id,
                TipoId = (int)tipo,
                Label = FormatLabel(tipo),
                Semestre = doc?.Semestre ?? semester,
                Estado = (doc != null && !string.IsNullOrEmpty(doc.RutaArchivo)) ? (int)doc.Estado : -1,
                EstadoText = GetEstadoText((doc != null && !string.IsNullOrEmpty(doc.RutaArchivo)) ? doc.Estado : null),
                EstadoSeverity = GetEstadoSeverity((doc != null && !string.IsNullOrEmpty(doc.RutaArchivo)) ? doc.Estado : null),
                Version = doc?.Version ?? 0,
                FechaLimite = fechaLimite,
                PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles),
                EsAcuerdo = true,
                Archivo = (doc != null && !string.IsNullOrEmpty(doc.RutaArchivo)) ? new FileDetailsDto { Id = doc.Id, FechaSubida = doc.FechaSubida ?? DateTime.MinValue } : null
            });
        }

        foreach (TipoDocumentoAlumno tipo in Enum.GetValues(typeof(TipoDocumentoAlumno)))
        {
            var doc = docsAlumnos.FirstOrDefault(d => d.TipoDocumento == tipo);

            resultList.Add(new ExpedienteItemDto
            {
                DocumentoId = doc?.Id,
                TipoId = (int)tipo,
                Label = FormatLabel(tipo),
                Semestre = doc?.Semestre ?? semester,
                Estado = doc != null ? (int)doc.Estado : -1,
                EstadoText = GetEstadoText(doc?.Estado),
                EstadoSeverity = GetEstadoSeverity(doc?.Estado),
                Version = doc?.Version ?? 0,
                FechaLimite = null,
                PuedeSubir = DocumentoPermissions.CanUpload(tipo, roles),
                EsAcuerdo = false,
                Archivo = doc != null ? new FileDetailsDto { Id = doc.Id, FechaSubida = doc.FechaSubida } : null
            });
        }

        await Result<List<ExpedienteItemDto>>.Success(resultList).ToResult().ExecuteAsync(HttpContext);
    }

    private string FormatLabel(TipoAcuerdo tipo) => tipo.ToString().Replace("Anexo", "Anexo ");
    private string FormatLabel(TipoDocumentoAlumno tipo) => tipo.ToString();

    private string GetEstadoText(EstadoDocumento? estado) => estado switch
    {
        EstadoDocumento.Aprobado => "Aprobado",
        EstadoDocumento.Rechazado => "Rechazado",
        EstadoDocumento.PendienteRevision => "Pendiente",
        _ => "No subido"
    };

    private string GetEstadoSeverity(EstadoDocumento? estado) => estado switch
    {
        EstadoDocumento.Aprobado => "success",
        EstadoDocumento.Rechazado => "danger",
        EstadoDocumento.PendienteRevision => "warning",
        _ => "secondary"
    };
}
