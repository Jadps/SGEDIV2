using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

namespace BACKSGEDI.Features.Auth.Logout;

public class LogoutEndpoint : EndpointWithoutRequest<object>
{
    private readonly ApplicationDbContext _db;

    public LogoutEndpoint(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/auth/logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _db.SaveChangesAsync(ct);
            }
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddYears(-1)
        };

        HttpContext.Response.Cookies.Delete("AccessToken", cookieOptions);
        HttpContext.Response.Cookies.Delete("RefreshToken", cookieOptions);
        HttpContext.Response.Cookies.Delete("XSRF-TOKEN", cookieOptions);

        await Result<object>.Success(new { message = "Logged out successfully" }).ToResult().ExecuteAsync(HttpContext);
    }
}
