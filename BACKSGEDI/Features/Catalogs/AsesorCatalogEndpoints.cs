using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Catalogs;

public record AsesorInternoCatalogDto(Guid ProfileId, Guid UsuarioId, string Name, string Email, string? NumeroEmpleado, string? Cubiculo);
public record AsesorExternoCatalogDto(Guid ProfileId, Guid UsuarioId, string Name, string Email, string EmpresaNombre, string Puesto);

public class GetAsesorInternosCatalogEndpoint : EndpointWithoutRequest<List<AsesorInternoCatalogDto>>
{
    private readonly ApplicationDbContext _db;
    public GetAsesorInternosCatalogEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/catalogs/asesores-internos");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var asesores = await _db.AsesoresInternos
            .AsNoTracking()
            .Select(a => new AsesorInternoCatalogDto(
                a.Id,
                a.UsuarioId,
                a.Usuario!.Name,
                a.Usuario.Email,
                a.NumeroEmpleado,
                a.Cubiculo
            ))
            .ToListAsync(ct);

        await Result<List<AsesorInternoCatalogDto>>.Success(asesores).ToResult().ExecuteAsync(HttpContext);
    }
}

public class GetAsesorExternosCatalogEndpoint : EndpointWithoutRequest<List<AsesorExternoCatalogDto>>
{
    private readonly ApplicationDbContext _db;
    public GetAsesorExternosCatalogEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/catalogs/asesores-externos");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var empresaId = Query<Guid?>("empresaId");

        var query = _db.AsesoresExternos.AsNoTracking();

        if (empresaId.HasValue && empresaId.Value != Guid.Empty)
            query = query.Where(a => a.EmpresaId == empresaId.Value);

        var asesores = await query
            .Select(a => new AsesorExternoCatalogDto(
                a.Id,
                a.UsuarioId,
                a.Usuario!.Name,
                a.Usuario.Email,
                a.Empresa!.Nombre,
                a.Puesto
            ))
            .ToListAsync(ct);

        await Result<List<AsesorExternoCatalogDto>>.Success(asesores).ToResult().ExecuteAsync(HttpContext);
    }
}

public record ProfesorCatalogDto(Guid ProfileId, Guid UsuarioId, string Name, string Email, int? CarreraId, string? CarreraNombre, string? NumeroEmpleado);

public class GetProfesoresCatalogEndpoint : EndpointWithoutRequest<List<ProfesorCatalogDto>>
{
    private readonly ApplicationDbContext _db;
    public GetProfesoresCatalogEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/catalogs/profesores");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var carreraId = Query<int?>("carreraId");

        var query = _db.Profesores.AsNoTracking();

        var profesores = await query
            .Select(p => new ProfesorCatalogDto(
                p.Id,
                p.UsuarioId,
                p.Usuario!.Name,
                p.Usuario.Email,
                null,
                null,
                null
            ))
            .ToListAsync(ct);

        await Result<List<ProfesorCatalogDto>>.Success(profesores).ToResult().ExecuteAsync(HttpContext);
    }
}
