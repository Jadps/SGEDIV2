using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BACKSGEDI.Configuration;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;

namespace BACKSGEDI.Features.Auth.Refresh;

public class RefreshTokenEndpoint : EndpointWithoutRequest<object>
{
    private readonly ApplicationDbContext _db;
    private readonly JwtOptions _jwtOptions;
    private readonly IAntiforgery _antiforgery;

    public RefreshTokenEndpoint(ApplicationDbContext db, IOptions<JwtOptions> jwtOptions, IAntiforgery antiforgery)
    {
        _db = db;
        _jwtOptions = jwtOptions.Value;
        _antiforgery = antiforgery;
    }

    public override void Configure()
    {
        Post("/api/auth/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var oldRefreshToken) || string.IsNullOrEmpty(oldRefreshToken))
        {
            await Result.Failure(Error.Unauthorized("Auth.InvalidToken", "No se proporcionó un Refresh Token.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var user = await _db.Usuarios
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.RefreshToken == oldRefreshToken, ct);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            await Result.Failure(Error.Unauthorized("Auth.InvalidToken", "El Refresh Token es inválido o ha expirado.")).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var newRefreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _db.SaveChangesAsync(ct);

        var accessToken = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = _jwtOptions.SecretKey;
            o.ExpireAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);
            foreach(var ur in user.Roles)
            {
                o.User.Roles.Add(ur.Role);
            }
            o.User.Claims.Add((ClaimTypes.NameIdentifier, user.Id.ToString()));
            o.User.Claims.Add((ClaimTypes.Name, user.Name));
            o.User.Claims.Add((ClaimTypes.Email, user.Email));
        });

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes) 
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.None,
            Expires = user.RefreshTokenExpiryTime
        };

        HttpContext.Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
        HttpContext.Response.Cookies.Append("RefreshToken", newRefreshToken, refreshCookieOptions);

        _antiforgery.GetAndStoreTokens(HttpContext);

        await Result<object>.Success(new { message = "Session refreshed successfully" }).ToResult().ExecuteAsync(HttpContext);
    }
}
