using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Features.Catalogs.Carreras;

public record CarreraDto(int Id, string Clave, string Nombre);
public record CreateCarreraRequest(string Clave, string Nombre);
public record UpdateCarreraRequest(int Id, string Clave, string Nombre);

public class ListCarrerasEndpoint : EndpointWithoutRequest<List<CarreraDto>>
{
    private readonly ApplicationDbContext _db;
    public ListCarrerasEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/catalogs/carreras");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = _db.Carreras.IgnoreQueryFilters().AsQueryable();

        if (User.Identity != null && User.Identity.IsAuthenticated && !User.GetRoles().Contains(SystemRoles.Admin))
        {
            var allowedCarreraId = await User.GetAllowedCarreraIdAsync(_db, ct);
            if (allowedCarreraId.HasValue)
            {
                query = query.Where(c => c.Id == allowedCarreraId.Value);
            }
        }

        var carreras = await query
            .Select(c => new CarreraDto(c.Id, c.Clave, c.Nombre))
            .ToListAsync(ct);

        await Result<List<CarreraDto>>.Success(carreras).ToResult().ExecuteAsync(HttpContext);
    }
}

public class CreateCarreraEndpoint : Endpoint<CreateCarreraRequest, CarreraDto>
{
    private readonly ApplicationDbContext _db;
    public CreateCarreraEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/catalogs/carreras");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(CreateCarreraRequest req, CancellationToken ct)
    {
        var lastId = await _db.Carreras.MaxAsync(c => (int?)c.Id, ct) ?? 0;
        var carrera = new CatCarrera
        {
            Id = lastId + 1,
            Clave = req.Clave,
            Nombre = req.Nombre
        };

        _db.Carreras.Add(carrera);
        await _db.SaveChangesAsync(ct);

        var dto = new CarreraDto(carrera.Id, carrera.Clave, carrera.Nombre);
        await Result<CarreraDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class UpdateCarreraEndpoint : Endpoint<UpdateCarreraRequest, CarreraDto>
{
    private readonly ApplicationDbContext _db;
    public UpdateCarreraEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/api/catalogs/carreras");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(UpdateCarreraRequest req, CancellationToken ct)
    {
        var carrera = await _db.Carreras.FirstOrDefaultAsync(c => c.Id == req.Id, ct);
        if (carrera == null)
        {
            await Result.Failure(Error.NotFound("Carrera.NotFound", "Carrera no encontrada")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        carrera.Clave = req.Clave;
        carrera.Nombre = req.Nombre;

        await _db.SaveChangesAsync(ct);

        var dto = new CarreraDto(carrera.Id, carrera.Clave, carrera.Nombre);
        await Result<CarreraDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class DeleteCarreraEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    public DeleteCarreraEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/api/catalogs/carreras/{id}");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var carrera = await _db.Carreras.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (carrera == null)
        {
            await Result.Failure(Error.NotFound("Carrera.NotFound", "Carrera no encontrada")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        carrera.Status = (int)EntityStatus.Borrado;
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
