using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Infrastructure.Extensions;

public static class UserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    public static string GetName(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public static List<string> GetRoles(this ClaimsPrincipal user) =>
        user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

    public static int GetStatus(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("status");
        return claim != null && int.TryParse(claim.Value, out var status) ? status : 1;
    }

    public static async Task<int?> GetAllowedCarreraIdAsync(this ClaimsPrincipal user, BACKSGEDI.Infrastructure.Data.ApplicationDbContext db, CancellationToken ct)
    {
        var userId = user.GetUserId();
        if (userId == Guid.Empty) return null;

        return await db.Usuarios
            .Where(u => u.Id == userId)
            .Select(u => u.Coordinador != null ? u.Coordinador.CarreraId :
                         u.JefeDepartamento != null ? u.JefeDepartamento.CarreraId :
                         u.Alumno != null ? u.Alumno.CarreraId : (int?)null)
            .FirstOrDefaultAsync(ct);
    }
}
