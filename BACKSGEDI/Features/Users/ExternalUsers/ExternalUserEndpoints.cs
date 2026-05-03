using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Features.Users.ExternalUsers;

public record ExternalUserDto(Guid Id, string Name, string Email, Guid EmpresaId, string EmpresaNombre, string Puesto, string TelefonoOficina, int Status);
public record CreateExternalUserRequest(string Name, string Email, string? Password, Guid EmpresaId, string Puesto, string TelefonoOficina);
public record UpdateExternalUserRequest(Guid Id, string Name, string Email, string Puesto, string TelefonoOficina);

public class ListExternalUsersEndpoint : EndpointWithoutRequest<List<ExternalUserDto>>
{
    private readonly ApplicationDbContext _db;
    public ListExternalUsersEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/users/external");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var empresaId = Query<Guid?>("empresaId");

        var query = _db.Usuarios
            .Where(u => u.AsesorExterno != null);

        if (empresaId.HasValue && empresaId.Value != Guid.Empty)
        {
            query = query.Where(u => u.AsesorExterno!.EmpresaId == empresaId.Value);
        }

        var users = await query
            .Select(u => new ExternalUserDto(
                u.Id,
                u.Name,
                u.Email,
                u.AsesorExterno!.EmpresaId,
                u.AsesorExterno.Empresa!.Nombre,
                u.AsesorExterno.Puesto,
                u.AsesorExterno.TelefonoOficina,
                u.Status
            ))
            .ToListAsync(ct);

        await Result<List<ExternalUserDto>>.Success(users).ToResult().ExecuteAsync(HttpContext);
    }
}

public record GetExternalUserRequest(Guid Id);

public class GetExternalUserEndpoint : Endpoint<GetExternalUserRequest, ExternalUserDto>
{
    private readonly ApplicationDbContext _db;
    public GetExternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/users/external/{Id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(GetExternalUserRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .Where(u => u.Id == req.Id && u.AsesorExterno != null)
            .Select(u => new ExternalUserDto(
                u.Id,
                u.Name,
                u.Email,
                u.AsesorExterno!.EmpresaId,
                u.AsesorExterno.Empresa!.Nombre,
                u.AsesorExterno.Puesto,
                u.AsesorExterno.TelefonoOficina,
                u.Status
            ))
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            await Result.Failure(Error.NotFound("User.NotFound", "Asesor no encontrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Result<ExternalUserDto>.Success(user).ToResult().ExecuteAsync(HttpContext);
    }
}

public class CreateExternalUserEndpoint : Endpoint<CreateExternalUserRequest, ExternalUserDto>
{
    private readonly ApplicationDbContext _db;
    public CreateExternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/users/external");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CreateExternalUserRequest req, CancellationToken ct)
    {
        var emailExists = await _db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == req.Email, ct);
        if (emailExists)
        {
            await Result.Failure(Error.Conflict("User.EmailExists", "El correo ya está registrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var user = new Usuario
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = string.IsNullOrEmpty(req.Password) ? "" : BCrypt.Net.BCrypt.HashPassword(req.Password),
            Roles = new List<UsuarioRol> { new UsuarioRol { Role = SystemRoles.AsesorExterno } },
            Status = (int)EntityStatus.Activo
        };

        _db.Usuarios.Add(user);

        var asesorExterno = new AsesorExterno
        {
            UsuarioId = user.Id,
            EmpresaId = req.EmpresaId,
            Puesto = req.Puesto,
            TelefonoOficina = req.TelefonoOficina
        };

        _db.AsesoresExternos.Add(asesorExterno);
        await _db.SaveChangesAsync(ct);

        var empresaNombre = await _db.Empresas.Where(e => e.Id == req.EmpresaId).Select(e => e.Nombre).FirstOrDefaultAsync(ct) ?? "";
        var dto = new ExternalUserDto(user.Id, user.Name, user.Email, req.EmpresaId, empresaNombre, req.Puesto, req.TelefonoOficina, user.Status);
        
        await Result<ExternalUserDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class UpdateExternalUserEndpoint : Endpoint<UpdateExternalUserRequest, ExternalUserDto>
{
    private readonly ApplicationDbContext _db;
    public UpdateExternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/api/users/external");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(UpdateExternalUserRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .Include(u => u.AsesorExterno)
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (user == null || user.AsesorExterno == null)
        {
            await Result.Failure(Error.NotFound("User.NotFound", "Asesor no encontrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        user.Name = req.Name;
        user.Email = req.Email;
        user.AsesorExterno.Puesto = req.Puesto;
        user.AsesorExterno.TelefonoOficina = req.TelefonoOficina;

        await _db.SaveChangesAsync(ct);

        var empresaNombre = await _db.Empresas.Where(e => e.Id == user.AsesorExterno.EmpresaId).Select(e => e.Nombre).FirstOrDefaultAsync(ct) ?? "";
        var dto = new ExternalUserDto(user.Id, user.Name, user.Email, user.AsesorExterno.EmpresaId, empresaNombre, user.AsesorExterno.Puesto, user.AsesorExterno.TelefonoOficina, user.Status);

        await Result<ExternalUserDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class DeleteExternalUserEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    public DeleteExternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/api/users/external/{id}");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
        {
            await Result.Failure(Error.NotFound("User.NotFound", "Asesor no encontrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        user.Status = (int)EntityStatus.Borrado;
        await _db.SaveChangesAsync(ct);

        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
