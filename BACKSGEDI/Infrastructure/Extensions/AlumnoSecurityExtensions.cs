using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Infrastructure.Extensions;

public static class AlumnoSecurityExtensions
{
    public static IQueryable<Alumno> ApplySecurityFilter(
        this IQueryable<Alumno> query, 
        Guid userId, 
        List<string> roles,
        ApplicationDbContext db)
    {
        if (roles.Contains(SystemRoles.Admin)) 
            return query;
        var isCoordinador = roles.Contains(SystemRoles.Coordinador);
        var isJefe = roles.Contains(SystemRoles.JefeDepartamento);
        var isAlumno = roles.Contains(SystemRoles.Alumno);

        return query.Where(a => 
            (isAlumno && a.UsuarioId == userId) ||
            (isCoordinador && db.Coordinadores.Any(c => c.UsuarioId == userId && c.CarreraId == a.CarreraId)) ||
            (isJefe && db.JefesDepartamento.Any(j => j.UsuarioId == userId && j.CarreraId == a.CarreraId))
            
            );
    }
}