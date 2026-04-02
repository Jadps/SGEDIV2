using System.Security.Claims;
using FastEndpoints;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Users.Me;

public class MeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class GetMeEndpoint : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/api/users/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var nameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value 
                        ?? User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "Token inválido o no provisto."))
                .ToResult()
                .ExecuteAsync(HttpContext);
            return;
        }

        var response = new MeResponse
        {
            Id = userId,
            Name = nameClaim ?? string.Empty,
            Email = emailClaim ?? string.Empty,
            Role = roleClaim ?? string.Empty
        };

        await Result<MeResponse>.Success(response).ToResult().ExecuteAsync(HttpContext);
    }
}
