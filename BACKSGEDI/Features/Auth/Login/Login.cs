using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BACKSGEDI.Configuration;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BACKSGEDI.Features.Auth.Login;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public string RefreshToken { get; set; } = string.Empty;
}

public class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.")
            .EmailAddress().WithMessage("Formato de email incorrecto.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.");
    }
}

public class LoginEndpoint : FastEndpoints.Endpoint<LoginRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IAntiforgery _antiforgery;
    private readonly JwtOptions _jwtOptions;

    public LoginEndpoint(ApplicationDbContext db, IAntiforgery antiforgery, IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _antiforgery = antiforgery;
        _jwtOptions = jwtOptions.Value;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await LoginAsync(req, ct);

        if (result.IsFailure)
        {
            await result.ToResult().ExecuteAsync(HttpContext);
            return;
        }

        SetupAuthCookies(result.Value);
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.CompleteAsync();
    }

    private async Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas."));

        if (!user.IsActive)
            return Result<LoginResponse>.Failure(Error.Unauthorized("Auth.AccountInactive", "Tu cuenta aún no ha sido activada. Contacta a tu coordinador."));
 
        var refreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _db.SaveChangesAsync(ct);

        return new LoginResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Roles = user.Roles.Select(r => r.Role).ToList(),
            RefreshToken = refreshToken
        };
    }

    private void SetupAuthCookies(LoginResponse user)
    {
        var accessToken = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = _jwtOptions.SecretKey;
            o.ExpireAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);
            foreach(var r in user.Roles)
            {
                o.User.Roles.Add(r);
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
            Expires = DateTimeOffset.UtcNow.AddMinutes(15) 
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7) 
        };

        HttpContext.Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
        HttpContext.Response.Cookies.Append("RefreshToken", user.RefreshToken, refreshCookieOptions);

        _antiforgery.GetAndStoreTokens(HttpContext);
    }
}
