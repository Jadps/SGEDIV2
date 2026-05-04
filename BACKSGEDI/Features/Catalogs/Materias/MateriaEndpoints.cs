using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Features.Catalogs.Materias;

public record MateriaDto(Guid Id, string Clave, string Nombre, int Creditos, int Semestre, int CarreraId, string CarreraNombre);
public record CreateMateriaRequest(string Clave, string Nombre, int Creditos, int Semestre, int CarreraId);
public record UpdateMateriaRequest(Guid Id, string Clave, string Nombre, int Creditos, int Semestre, int CarreraId);
public record ListMateriasRequest
{
    [QueryParam]
    public int? CarreraId { get; init; }
}

public class ListMateriasEndpoint : Endpoint<ListMateriasRequest, List<MateriaDto>>
{
    private readonly ApplicationDbContext _db;
    public ListMateriasEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/catalogs/materias");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.Alumno);
    }

    public override async Task HandleAsync(ListMateriasRequest req, CancellationToken ct)
    {
        var query = _db.Materias.AsQueryable();
        int? filterCarreraId = req.CarreraId;

        if (!User.GetRoles().Contains(SystemRoles.Admin))
        {
            var filterCarreraIdFromDb = await User.GetAllowedCarreraIdAsync(_db, ct);
            if (filterCarreraIdFromDb.HasValue)
                filterCarreraId = filterCarreraIdFromDb.Value;
        }

        if (filterCarreraId.HasValue && filterCarreraId.Value > 0)
        {
            query = query.Where(m => m.CarreraId == filterCarreraId.Value);
        }

        var materias = await query
            .Select(m => new MateriaDto(m.Id, m.Clave, m.Nombre, m.Creditos, m.Semestre, m.CarreraId, m.Carrera!.Nombre))
            .ToListAsync(ct);

        await Result<List<MateriaDto>>.Success(materias).ToResult().ExecuteAsync(HttpContext);
    }
}

public class CreateMateriaEndpoint : Endpoint<CreateMateriaRequest, MateriaDto>
{
    private readonly ApplicationDbContext _db;
    public CreateMateriaEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/catalogs/materias");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CreateMateriaRequest req, CancellationToken ct)
    {
        var carreraId = req.CarreraId;

        if (!User.GetRoles().Contains(SystemRoles.Admin))
        {
            var allowedCarreraId = await User.GetAllowedCarreraIdAsync(_db, ct);
            if (allowedCarreraId.HasValue)
                carreraId = allowedCarreraId.Value;
        }

        var materia = new Materia
        {
            Clave = req.Clave,
            Nombre = req.Nombre,
            Creditos = req.Creditos,
            Semestre = req.Semestre,
            CarreraId = carreraId
        };

        _db.Materias.Add(materia);
        await _db.SaveChangesAsync(ct);

        var carreraNombre = await _db.Carreras.Where(c => c.Id == carreraId).Select(c => c.Nombre).FirstOrDefaultAsync(ct) ?? "";
        var dto = new MateriaDto(materia.Id, materia.Clave, materia.Nombre, materia.Creditos, materia.Semestre, materia.CarreraId, carreraNombre);
        await Result<MateriaDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class UpdateMateriaEndpoint : Endpoint<UpdateMateriaRequest, MateriaDto>
{
    private readonly ApplicationDbContext _db;
    public UpdateMateriaEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/api/catalogs/materias");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(UpdateMateriaRequest req, CancellationToken ct)
    {
        var materia = await _db.Materias.FirstOrDefaultAsync(m => m.Id == req.Id, ct);
        if (materia == null)
        {
            await Result.Failure(Error.NotFound("Materia.NotFound", "Materia no encontrada")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var newCarreraId = req.CarreraId;

        if (!User.GetRoles().Contains(SystemRoles.Admin))
        {
            var allowedCarreraId = (await User.GetAllowedCarreraIdAsync(_db, ct)) ?? 0;

            if (materia.CarreraId != allowedCarreraId)
            {
                await Result.Failure(Error.Forbidden("Materia.Forbidden", "No tienes permisos para modificar materias de otra carrera")).ToResult().ExecuteAsync(HttpContext);
                return;
            }
            newCarreraId = allowedCarreraId;
        }

        materia.Clave = req.Clave;
        materia.Nombre = req.Nombre;
        materia.Creditos = req.Creditos;
        materia.Semestre = req.Semestre;
        materia.CarreraId = newCarreraId;

        await _db.SaveChangesAsync(ct);

        var carreraNombre = await _db.Carreras.Where(c => c.Id == newCarreraId).Select(c => c.Nombre).FirstOrDefaultAsync(ct) ?? "";
        var dto = new MateriaDto(materia.Id, materia.Clave, materia.Nombre, materia.Creditos, materia.Semestre, materia.CarreraId, carreraNombre);
        await Result<MateriaDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class DeleteMateriaEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    public DeleteMateriaEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/api/catalogs/materias/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var materia = await _db.Materias.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (materia == null)
        {
            await Result.Failure(Error.NotFound("Materia.NotFound", "Materia no encontrada")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        if (!User.GetRoles().Contains(SystemRoles.Admin))
        {
            var allowedCarreraId = (await User.GetAllowedCarreraIdAsync(_db, ct)) ?? 0;

            if (materia.CarreraId != allowedCarreraId)
            {
                await Result.Failure(Error.Forbidden("Materia.Forbidden", "No tienes permisos para eliminar materias de otra carrera")).ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }

        materia.Status = (int)EntityStatus.Borrado;
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
