using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BACKSGEDI.Features.Documents.ListStudentDocuments;

public record StudentDocumentDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Version { get; set; }
    public EstadoDocumento Estado { get; set; }
    public DateTime? FechaSubida { get; set; }
    public string Semestre { get; set; } = string.Empty;
    public bool EsAcuerdo { get; set; }
    public DateTime? FechaLimite { get; set; }
    public int TipoId { get; set; }
}

public record ListStudentDocumentsRequest
{
    public Guid AlumnoId { get; set; }
    [QueryParam] public string? Semestre { get; set; }
}

public class ListStudentDocuments : Endpoint<ListStudentDocumentsRequest, List<StudentDocumentDto>>
{
    private readonly ApplicationDbContext _db;

    public ListStudentDocuments(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/alumnos/{alumnoId}/documentos");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, 
              SystemRoles.Alumno, SystemRoles.Profesor, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno);
    }

    public override async Task HandleAsync(ListStudentDocumentsRequest req, CancellationToken ct)
    {
        var alumnoId = req.AlumnoId;
        var requesterId = User.GetUserId();
        var roles = User.GetRoles();
        
        var isAdmin = roles.Contains(SystemRoles.Admin);
        var isCoord = roles.Contains(SystemRoles.Coordinador);
        var isJefe = roles.Contains(SystemRoles.JefeDepartamento);
        var isAlumno = roles.Contains(SystemRoles.Alumno);
        
        if (isAlumno)
        {
            var isMe = await _db.Alumnos.AnyAsync(a => a.Id == alumnoId && a.UsuarioId == requesterId, ct);
            if (!isMe) {
                await Result.Failure(Error.Forbidden("Doc.Forbidden", "No puedes subir documentos de otro alumno."))
    .ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }
        
        var semestreActual = SemestreHelper.GetSemestreActual();
        var seeHistorical = isAdmin || isCoord || isJefe;

        var personalDocsQuery = _db.DocumentosAlumnos
            .AsNoTracking()
            .Where(d => d.AlumnoId == alumnoId && d.EsVersionActual);

        var acuerdoDocsQuery = _db.DocumentosAcuerdos
            .AsNoTracking()
            .Where(d => d.AlumnoId == alumnoId && d.EsVersionActual);

        if (!string.IsNullOrEmpty(req.Semestre))
        {
            personalDocsQuery = personalDocsQuery.Where(d => d.Semestre == req.Semestre);
            acuerdoDocsQuery = acuerdoDocsQuery.Where(d => d.Semestre == req.Semestre);
        }
        else if (!seeHistorical)
        {
            personalDocsQuery = personalDocsQuery.Where(d => d.Semestre == semestreActual);
            acuerdoDocsQuery = acuerdoDocsQuery.Where(d => d.Semestre == semestreActual);
        }

        var personalDocs = await personalDocsQuery
            .Select(d => new StudentDocumentDto
            {
                Id = d.Id,
                Tipo = d.TipoDocumento.ToString(),
                Version = d.Version,
                Estado = d.Estado,
                FechaSubida = d.FechaSubida,
                Semestre = d.Semestre,
                EsAcuerdo = false,
                TipoId = (int)d.TipoDocumento
            }).ToListAsync(ct);

        var acuerdoDocs = await acuerdoDocsQuery
            .Select(d => new StudentDocumentDto
            {
                Id = d.Id,
                Tipo = d.TipoAcuerdo.ToString(),
                Version = d.Version,
                Estado = d.Estado,
                FechaSubida = d.FechaSubida,
                Semestre = d.Semestre,
                EsAcuerdo = true,
                FechaLimite = d.FechaLimite,
                TipoId = (int)d.TipoAcuerdo
            }).ToListAsync(ct);

        var result = personalDocs.Union(acuerdoDocs)
            .OrderByDescending(d => d.Semestre)
            .ThenBy(d => d.Tipo)
            .ToList();

        await Result<List<StudentDocumentDto>>.Success(result)
            .ToResult().ExecuteAsync(HttpContext);
    }
}

