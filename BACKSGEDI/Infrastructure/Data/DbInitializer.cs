using BCrypt.Net;
using BACKSGEDI.Domain.Enums;
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

        var carreras = new List<CatCarrera>
        {
            new CatCarrera { Id = 1, Clave = "IGE", Nombre = "Ingeniería en Gestión Empresarial", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 2, Clave = "ITIC", Nombre = "Ingeniería en Tecnologías de la Información y Comunicaciones", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 3, Clave = "II", Nombre = "Ingeniería Industrial", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 4, Clave = "IA", Nombre = "Ingeniería en Administración", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 5, Clave = "IDA", Nombre = "Ingeniería en Desarrollo de Aplicaciones", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 6, Clave = "IE", Nombre = "Ingeniería Eléctrica", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 7, Clave = "IEM", Nombre = "Ingeniería Electromecánica", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 8, Clave = "IEL", Nombre = "Ingeniería Electrónica", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 9, Clave = "IM", Nombre = "Ingeniería Mecánica", Status = (int)EntityStatus.Activo },
            new CatCarrera { Id = 10, Clave = "IMT", Nombre = "Ingeniería Mecatrónica", Status = (int)EntityStatus.Activo }
        };

        foreach (var carrera in carreras)
        {
            var exists = await context.Carreras.IgnoreQueryFilters().AnyAsync(c => c.Id == carrera.Id);
            if (!exists)
            {
                context.Carreras.Add(carrera);
            }
        }

        await context.SaveChangesAsync();
    }
}
