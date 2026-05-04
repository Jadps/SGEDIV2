using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Features.Alumnos.Get;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Alumnos.CargaAcademica;

public record SetCargaAcademicaRequest
{
    public List<MateriaProfesorRequest> Materias { get; set; } = new();
}

public record MateriaProfesorRequest
{
    public Guid MateriaId { get; set; }
    public Guid ProfesorId { get; set; }
}

public class SetCargaAcademica : Endpoint<SetCargaAcademicaRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly BACKSGEDI.Domain.Interfaces.IFechasLimiteService _fechasLimiteService;

    public SetCargaAcademica(ApplicationDbContext db, BACKSGEDI.Domain.Interfaces.IFechasLimiteService fechasLimiteService)
    {
        _db = db;
        _fechasLimiteService = fechasLimiteService;
    }

    public override void Configure()
    {
        Post("/api/alumnos/me/carga-academica");
        Roles(SystemRoles.Alumno);
    }

    public override async Task HandleAsync(SetCargaAcademicaRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var alumno = await _db.Alumnos
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.UsuarioId == userId, ct);

        if (alumno == null)
        {
            await Result.Failure(Error.NotFound("Alumno.NotFound", "No se encontró el perfil del alumno.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (req.Materias.Count > 8)
        {
            await Result.Failure(Error.Validation("CargaAcademica.MaxMaterias", "No se permiten más de 8 materias por semestre.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var distinctMaterias = req.Materias.Select(m => m.MateriaId).Distinct().Count();
        if (distinctMaterias != req.Materias.Count)
        {
            await Result.Failure(Error.Validation("CargaAcademica.Duplicadas", "No se permiten materias duplicadas en la carga académica.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var materiaIds = req.Materias.Select(m => m.MateriaId).ToList();
        var totalCreditos = await _db.Materias
            .Where(m => materiaIds.Contains(m.Id))
            .SumAsync(m => m.Creditos, ct);

        if (totalCreditos > 36)
        {
            await Result.Failure(Error.Validation("CargaAcademica.MaxCreditos", $"La carga excede el límite de 36 créditos. (Total intentado: {totalCreditos})")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var semestreActual = SemestreHelper.GetSemestreActual();

        var cargaAnterior = await _db.CargasAcademicas
            .Where(ca => ca.AlumnoId == alumno.Id && ca.Semestre == semestreActual)
            .ToListAsync(ct);
        
        if (cargaAnterior.Any())
        {
            _db.CargasAcademicas.RemoveRange(cargaAnterior);
            
            var anexosPlaceholders = await _db.DocumentosAcuerdos
                .Where(a => a.AlumnoId == alumno.Id && (a.TipoAcuerdo == TipoAcuerdo.AnexoIII || a.TipoAcuerdo == TipoAcuerdo.AnexoVII) && a.Semestre == semestreActual && a.RutaArchivo == null)
                .ToListAsync(ct);
            _db.DocumentosAcuerdos.RemoveRange(anexosPlaceholders);
        }

        var nuevasMaterias = req.Materias.Select(m => new Domain.Entities.CargaAcademica
        {
            AlumnoId = alumno.Id,
            MateriaId = m.MateriaId,
            ProfesorId = m.ProfesorId,
            Semestre = semestreActual,
            Status = (int)EntityStatus.Activo
        }).ToList();

        await _db.CargasAcademicas.AddRangeAsync(nuevasMaterias, ct);

        var fechaLimiteAnexo3 = await _fechasLimiteService.GetFechaLimiteAsync(TipoAcuerdo.AnexoIII, alumno.CarreraId, semestreActual, ct);
        var fechaLimiteAnexo7 = await _fechasLimiteService.GetFechaLimiteAsync(TipoAcuerdo.AnexoVII, alumno.CarreraId, semestreActual, ct);
        
        foreach (var materia in nuevasMaterias)
        {
            var existe3 = await _db.DocumentosAcuerdos.AnyAsync(a => 
                a.AlumnoId == alumno.Id && 
                a.TipoAcuerdo == TipoAcuerdo.AnexoIII && 
                a.ProfesorId == materia.ProfesorId && 
                a.MateriaId == materia.MateriaId && 
                a.Semestre == semestreActual && 
                a.RutaArchivo != null, ct);

            if (!existe3)
            {
                await _db.DocumentosAcuerdos.AddAsync(new DocumentoAcuerdo
                {
                    AlumnoId = alumno.Id,
                    ProfesorId = materia.ProfesorId,
                    MateriaId = materia.MateriaId,
                    TipoAcuerdo = TipoAcuerdo.AnexoIII,
                    Semestre = semestreActual,
                    FechaLimite = fechaLimiteAnexo3,
                    Estado = EstadoDocumento.PendienteRevision,
                    SubidoPorUsuarioId = userId,
                    Version = 1,
                    EsVersionActual = true
                }, ct);
            }

            var existe7 = await _db.DocumentosAcuerdos.AnyAsync(a => 
                a.AlumnoId == alumno.Id && 
                a.TipoAcuerdo == TipoAcuerdo.AnexoVII && 
                a.ProfesorId == materia.ProfesorId && 
                a.MateriaId == materia.MateriaId && 
                a.Semestre == semestreActual && 
                a.RutaArchivo != null, ct);

            if (!existe7)
            {
                await _db.DocumentosAcuerdos.AddAsync(new DocumentoAcuerdo
                {
                    AlumnoId = alumno.Id,
                    ProfesorId = materia.ProfesorId,
                    MateriaId = materia.MateriaId,
                    TipoAcuerdo = TipoAcuerdo.AnexoVII,
                    Semestre = semestreActual,
                    FechaLimite = fechaLimiteAnexo7,
                    Estado = EstadoDocumento.PendienteRevision,
                    SubidoPorUsuarioId = userId,
                    Version = 1,
                    EsVersionActual = true
                }, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
