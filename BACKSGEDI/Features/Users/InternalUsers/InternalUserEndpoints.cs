using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Features.Users.InternalUsers;

public record InternalUserDto(Guid Id, string Name, string Email, List<string> Roles, int Status, int? CarreraId, string? CarreraNombre, string? NumeroEmpleado, string? Cubiculo);
public record CreateInternalUserRequest(string Name, string Email, string Password, List<string> Roles, int? CarreraId, string? NumeroEmpleado, string? Cubiculo);
public record UpdateInternalUserRequest(Guid Id, string Name, string Email, List<string> Roles, int Status, int? CarreraId, string? NumeroEmpleado, string? Cubiculo);

public class ListInternalUsersEndpoint : EndpointWithoutRequest<List<InternalUserDto>>
{
    private readonly ApplicationDbContext _db;
    public ListInternalUsersEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/users/internal");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _db.Usuarios
            .Where(u => u.Roles.Any(r => r.Role != SystemRoles.Alumno && r.Role != SystemRoles.AsesorExterno))
            .Select(u => new InternalUserDto(
                u.Id,
                u.Name,
                u.Email,
                u.Roles.Select(r => r.Role).ToList(),
                u.Status,
                u.Coordinador != null ? u.Coordinador.CarreraId
                    : u.JefeDepartamento != null ? u.JefeDepartamento.CarreraId
                    : null,
                u.Coordinador != null ? u.Coordinador.Carrera.Nombre
                    : u.JefeDepartamento != null ? u.JefeDepartamento.Carrera.Nombre
                    : null,
                u.AsesorInterno != null ? u.AsesorInterno.NumeroEmpleado : null,
                u.AsesorInterno != null ? u.AsesorInterno.Cubiculo : null
            ))
            .ToListAsync(ct);

        await Result<List<InternalUserDto>>.Success(users).ToResult().ExecuteAsync(HttpContext);
    }
}

public record GetInternalUserRequest(Guid Id);

public class GetInternalUserEndpoint : Endpoint<GetInternalUserRequest, InternalUserDto>
{
    private readonly ApplicationDbContext _db;
    public GetInternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/users/internal/{Id}");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(GetInternalUserRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .Where(u => u.Id == req.Id)
            .Select(u => new InternalUserDto(
                u.Id,
                u.Name,
                u.Email,
                u.Roles.Select(r => r.Role).ToList(),
                u.Status,
                u.Coordinador != null ? u.Coordinador.CarreraId
                    : u.JefeDepartamento != null ? u.JefeDepartamento.CarreraId
                    : null,
                u.Coordinador != null ? u.Coordinador.Carrera.Nombre
                    : u.JefeDepartamento != null ? u.JefeDepartamento.Carrera.Nombre
                    : null,
                u.AsesorInterno != null ? u.AsesorInterno.NumeroEmpleado : null,
                u.AsesorInterno != null ? u.AsesorInterno.Cubiculo : null
            ))
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            await Result.Failure(Error.NotFound("User.NotFound", "Usuario no encontrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        await Result<InternalUserDto>.Success(user).ToResult().ExecuteAsync(HttpContext);
    }
}

public class CreateInternalUserEndpoint : Endpoint<CreateInternalUserRequest, InternalUserDto>
{
    private readonly ApplicationDbContext _db;
    public CreateInternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/users/internal");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(CreateInternalUserRequest req, CancellationToken ct)
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Roles = req.Roles.Select(r => new UsuarioRol { Role = r }).ToList(),
            Status = (int)EntityStatus.Activo
        };

        _db.Usuarios.Add(user);

        if (req.Roles.Contains(SystemRoles.Coordinador) && req.CarreraId.HasValue)
        {
            _db.Coordinadores.Add(new Coordinador { UsuarioId = user.Id, CarreraId = req.CarreraId.Value });
        }

        if (req.Roles.Contains(SystemRoles.JefeDepartamento) && req.CarreraId.HasValue)
        {
            _db.JefesDepartamento.Add(new JefeDepartamento { UsuarioId = user.Id, CarreraId = req.CarreraId.Value });
        }

        if (req.Roles.Contains(SystemRoles.Profesor))
        {
            _db.Profesores.Add(new Profesor 
            { 
                UsuarioId = user.Id,
                NumeroEmpleado = req.NumeroEmpleado ?? "",
                Cubiculo = req.Cubiculo ?? ""
            });
        }

        if (req.Roles.Contains(SystemRoles.AsesorInterno))
        {
            _db.AsesoresInternos.Add(new AsesorInterno 
            { 
                UsuarioId = user.Id, 
                NumeroEmpleado = req.NumeroEmpleado ?? "", 
                Cubiculo = req.Cubiculo ?? "" 
            });
        }

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        var carreraNombre = req.CarreraId.HasValue ? await _db.Carreras.Where(c => c.Id == req.CarreraId.Value).Select(c => c.Nombre).FirstOrDefaultAsync(ct) : null;
        var dto = new InternalUserDto(user.Id, user.Name, user.Email, req.Roles, user.Status, req.CarreraId, carreraNombre, req.NumeroEmpleado, req.Cubiculo);
        
        await Result<InternalUserDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public class UpdateInternalUserEndpoint : Endpoint<UpdateInternalUserRequest, InternalUserDto>
{
    private readonly ApplicationDbContext _db;
    public UpdateInternalUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/api/users/internal");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(UpdateInternalUserRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .Include(u => u.Roles)
            .Include(u => u.Coordinador)
            .Include(u => u.JefeDepartamento)
            .Include(u => u.Profesor)
            .Include(u => u.AsesorInterno)
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (user == null)
        {
            await Result.Failure(Error.NotFound("User.NotFound", "Usuario no encontrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        user.Name = req.Name;
        user.Email = req.Email;
        user.Status = req.Status;

        foreach (var r in user.Roles.ToList())
            _db.Entry(r).State = EntityState.Detached;

        if (req.Roles.Contains(SystemRoles.Coordinador) && req.CarreraId.HasValue)
        {
            if (user.Coordinador == null)
                _db.Coordinadores.Add(new Coordinador { UsuarioId = user.Id, CarreraId = req.CarreraId.Value });
            else
                user.Coordinador.CarreraId = req.CarreraId.Value;
        }
        else if (user.Coordinador != null)
        {
            _db.Coordinadores.Remove(user.Coordinador);
        }

        if (req.Roles.Contains(SystemRoles.JefeDepartamento) && req.CarreraId.HasValue)
        {
            if (user.JefeDepartamento == null)
                _db.JefesDepartamento.Add(new JefeDepartamento { UsuarioId = user.Id, CarreraId = req.CarreraId.Value });
            else
                user.JefeDepartamento.CarreraId = req.CarreraId.Value;
        }
        else if (user.JefeDepartamento != null)
        {
            _db.JefesDepartamento.Remove(user.JefeDepartamento);
        }

        if (req.Roles.Contains(SystemRoles.Profesor))
        {
            if (user.Profesor == null)
                _db.Profesores.Add(new Profesor 
                { 
                    UsuarioId = user.Id,
                    NumeroEmpleado = req.NumeroEmpleado ?? "",
                    Cubiculo = req.Cubiculo ?? ""
                });
            else
            {
                user.Profesor.NumeroEmpleado = req.NumeroEmpleado ?? "";
                user.Profesor.Cubiculo = req.Cubiculo ?? "";
            }
        }
        else if (user.Profesor != null)
        {
            _db.Profesores.Remove(user.Profesor);
        }

        if (req.Roles.Contains(SystemRoles.AsesorInterno))
        {
            if (user.AsesorInterno == null)
                _db.AsesoresInternos.Add(new AsesorInterno { UsuarioId = user.Id, NumeroEmpleado = req.NumeroEmpleado ?? "", Cubiculo = req.Cubiculo ?? "" });
            else
            {
                user.AsesorInterno.NumeroEmpleado = req.NumeroEmpleado ?? "";
                user.AsesorInterno.Cubiculo = req.Cubiculo ?? "";
            }
        }
        else if (user.AsesorInterno != null)
        {
            _db.AsesoresInternos.Remove(user.AsesorInterno);
        }

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            await _db.UsuariosRoles
                .Where(r => r.UsuarioId == user.Id)
                .ExecuteDeleteAsync(ct);

            foreach (var role in req.Roles)
                _db.UsuariosRoles.Add(new UsuarioRol { UsuarioId = user.Id, Role = role });

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        var carreraNombre = req.CarreraId.HasValue ? await _db.Carreras.Where(c => c.Id == req.CarreraId.Value).Select(c => c.Nombre).FirstOrDefaultAsync(ct) : null;
        var dto = new InternalUserDto(user.Id, user.Name, user.Email, req.Roles, user.Status, req.CarreraId, carreraNombre, req.NumeroEmpleado, req.Cubiculo);
        
        await Result<InternalUserDto>.Success(dto).ToResult().ExecuteAsync(HttpContext);
    }
}

public record ToggleInternalUserStatusRequest(Guid Id);

public class ToggleInternalUserStatusEndpoint : Endpoint<ToggleInternalUserStatusRequest>
{
    private readonly ApplicationDbContext _db;
    public ToggleInternalUserStatusEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/api/users/internal/{Id}/toggle-status");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(ToggleInternalUserStatusRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (user == null)
        {
            await Result.Failure(Error.NotFound("User.NotFound", "Usuario no encontrado")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        user.Status = user.Status == (int)EntityStatus.Activo ? (int)EntityStatus.SinActivar : (int)EntityStatus.Activo;

        await _db.SaveChangesAsync(ct);

        await Result<int>.Success(user.Status).ToResult().ExecuteAsync(HttpContext);
    }
}
