using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

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
    public string Role { get; set; } = string.Empty;
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

public class LoginEndpoint : FastEndpoints.Endpoint<LoginRequest, LoginResponse>
{
    private readonly ApplicationDbContext _db;
    private readonly IAntiforgery _antiforgery;

    public LoginEndpoint(ApplicationDbContext db, IAntiforgery antiforgery)
    {
        _db = db;
        _antiforgery = antiforgery;
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

        var user = result.Value;
        SetupAuthCookies(user);

        await result.ToResult().ExecuteAsync(HttpContext);
    }

    private async Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas."));
        }

        return new LoginResponse
        {
            Id = user.Id,
            Name = user.Name,
            Role = user.Role
        };
    }

    private void SetupAuthCookies(LoginResponse user)
    {
        var accessToken = "mock.jwt.acccess.firmado.pronto." + user.Id;
        var refreshToken = "mock.jwt.refresh." + Guid.NewGuid();

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
        HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, refreshCookieOptions);

        _antiforgery.GetAndStoreTokens(HttpContext);
    }
}
