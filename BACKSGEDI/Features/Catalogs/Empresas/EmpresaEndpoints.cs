using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Catalogs.Empresas;

public record EmpresaDto(Guid Id, string Nombre, string Rfc, string Direccion, string Telefono, string Correo);
public record CreateEmpresaRequest(string Nombre, string Rfc, string Direccion, string Telefono, string Correo);
public record UpdateEmpresaRequest(Guid Id, string Nombre, string Rfc, string Direccion, string Telefono, string Correo);

public class ListEmpresasEndpoint : EndpointWithoutRequest<List<EmpresaDto>>
{
    private readonly ApplicationDbContext _db;
    public ListEmpresasEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/catalogs/empresas");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var empresas = await _db.Empresas
            .Select(e => new EmpresaDto(e.Id, e.Nombre, e.Rfc, e.Direccion, e.Telefono, e.Correo))
            .ToListAsync(ct);

        await Result<List<EmpresaDto>>.Success(empresas).ToResult().ExecuteAsync(HttpContext);
    }
}

public class CreateEmpresaEndpoint : Endpoint<CreateEmpresaRequest, EmpresaDto>
{
    private readonly ApplicationDbContext _db;
    public CreateEmpresaEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/catalogs/empresas");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CreateEmpresaRequest req, CancellationToken ct)
    {
        var empresa = new Empresa
        {
            Nombre = req.Nombre,
            Rfc = req.Rfc,
            Direccion = req.Direccion,
            Telefono = req.Telefono,
            Correo = req.Correo
        };

        _db.Empresas.Add(empresa);
        await _db.SaveChangesAsync(ct);

        var dto = new EmpresaDto(empresa.Id, empresa.Nombre, empresa.Rfc, empresa.Direccion, empresa.Telefono, empresa.Correo);
        await Result<EmpresaDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class UpdateEmpresaEndpoint : Endpoint<UpdateEmpresaRequest, EmpresaDto>
{
    private readonly ApplicationDbContext _db;
    public UpdateEmpresaEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/api/catalogs/empresas");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(UpdateEmpresaRequest req, CancellationToken ct)
    {
        var empresa = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == req.Id, ct);
        if (empresa == null)
        {
            await Result.Failure(Error.NotFound("Empresa.NotFound", "Empresa no encontrada")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        empresa.Nombre = req.Nombre;
        empresa.Rfc = req.Rfc;
        empresa.Direccion = req.Direccion;
        empresa.Telefono = req.Telefono;
        empresa.Correo = req.Correo;

        await _db.SaveChangesAsync(ct);

        var dto = new EmpresaDto(empresa.Id, empresa.Nombre, empresa.Rfc, empresa.Direccion, empresa.Telefono, empresa.Correo);
        await Result<EmpresaDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class DeleteEmpresaEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    public DeleteEmpresaEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/api/catalogs/empresas/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var empresa = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (empresa == null)
        {
            await Result.Failure(Error.NotFound("Empresa.NotFound", "Empresa no encontrada")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        empresa.IsDeleted = true;
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
