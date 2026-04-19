using System.Security.Claims;

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
}
