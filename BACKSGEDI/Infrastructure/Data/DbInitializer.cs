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
            var adminExists = await context.Usuarios.AnyAsync(u => u.Email == adminConfig.Mail.Trim());
            if (!adminExists)
            {
                var adminUser = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrador",
                    Email = adminConfig.Mail.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig.Pass.Trim()),
                    Role = SystemRoles.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                context.Usuarios.Add(adminUser);
            }
        }

        var carreras = new List<CatCarrera>
        {
            new CatCarrera { Id = 1, Clave = "IGE", Nombre = "Ingeniería en Gestión Empresarial", IsDeleted = false },
            new CatCarrera { Id = 2, Clave = "ITIC", Nombre = "Ingeniería en Tecnologías de la Información y Comunicaciones", IsDeleted = false },
            new CatCarrera { Id = 3, Clave = "II", Nombre = "Ingeniería Industrial", IsDeleted = false },
            new CatCarrera { Id = 4, Clave = "IA", Nombre = "Ingeniería en Administración", IsDeleted = false },
            new CatCarrera { Id = 5, Clave = "IDA", Nombre = "Ingeniería en Desarrollo de Aplicaciones", IsDeleted = false },
            new CatCarrera { Id = 6, Clave = "IE", Nombre = "Ingeniería Eléctrica", IsDeleted = false },
            new CatCarrera { Id = 7, Clave = "IEM", Nombre = "Ingeniería Electromecánica", IsDeleted = false },
            new CatCarrera { Id = 8, Clave = "IEL", Nombre = "Ingeniería Electrónica", IsDeleted = false },
            new CatCarrera { Id = 9, Clave = "IM", Nombre = "Ingeniería Mecánica", IsDeleted = false },
            new CatCarrera { Id = 10, Clave = "IMT", Nombre = "Ingeniería Mecatrónica", IsDeleted = false }
        };

        foreach (var carrera in carreras)
        {
            var exists = await context.Carreras.AnyAsync(c => c.Id == carrera.Id);
            if (!exists)
            {
                context.Carreras.Add(carrera);
            }

            var emailCoord = $"Coordinador{carrera.Clave}@tlalnepantla.tecnm.mx";
            var coordExists = await context.Usuarios.AnyAsync(u => u.Email == emailCoord);
            if (!coordExists)
            {
                var currentYear = DateTime.UtcNow.Year.ToString();
                var passwordStr = $"{currentYear}{carrera.Clave}";

                var coordUser = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Name = $"Coordinador {carrera.Clave}",
                    Email = emailCoord,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordStr),
                    Role = SystemRoles.Coordinador,
                    CreatedAt = DateTime.UtcNow
                };

                context.Usuarios.Add(coordUser);

                var coordinadorEntity = new Coordinador
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = coordUser.Id,
                    CarreraId = carrera.Id
                };

                context.Coordinadores.Add(coordinadorEntity);
            }
        }

        await context.SaveChangesAsync();
    }
}
