using BCrypt.Net;
using BACKSGEDI.Configuration;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BACKSGEDI.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IOptions<AdminOptions> adminOptions)
    {
        var adminConfig = adminOptions.Value;

        if (!string.IsNullOrEmpty(adminConfig.Mail))
        {
            var adminExists = await context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == adminConfig.Mail.Trim());
            if (!adminExists)
            {
                var adminUser = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrador",
                    Email = adminConfig.Mail.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig.Pass.Trim()),
                    Roles = new List<UsuarioRol> 
                    { 
                        new UsuarioRol { Role = SystemRoles.Admin } 
                    },
                    CreatedAt = DateTime.UtcNow
                };

                context.Usuarios.Add(adminUser);
            }
        }


        var hildaEmail = "hilda.diaz@tlalnepantla.tecnm.mx";
        var hildaExists = await context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == hildaEmail);
        if (!hildaExists)
        {
            var hildaUser = new Usuario
            {
                Id = Guid.NewGuid(),
                Name = "Hilda Diaz",
                Email = hildaEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig.Pass.Trim()),
                Roles = new List<UsuarioRol>
                {
                    new UsuarioRol { Role = SystemRoles.AsesorInterno },
                    new UsuarioRol { Role = SystemRoles.Profesor }
                },
                CreatedAt = DateTime.UtcNow
            };

            context.Usuarios.Add(hildaUser);

            var asesorInterno = new AsesorInterno
            {
                Id = Guid.NewGuid(),
                UsuarioId = hildaUser.Id,
                NumeroEmpleado = "ITTLA-001",
                Cubiculo = "Cubículo A"
            };
            context.AsesoresInternos.Add(asesorInterno);

            var profesor = new Profesor
            {
                Id = Guid.NewGuid(),
                UsuarioId = hildaUser.Id
            };
            context.Profesores.Add(profesor);
        }

        await context.SaveChangesAsync();
    }
}
