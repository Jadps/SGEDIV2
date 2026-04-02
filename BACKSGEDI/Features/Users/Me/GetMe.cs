using System.Security.Claims;
using FastEndpoints;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Users.Me;

public class RoleResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class MeResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<RoleResponseDto> Roles { get; set; } = [];
    public int CatStatusAccountId { get; set; } = 1;
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
        var nameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                        ?? User.Claims.FirstOrDefault(c => c.Type == "role")?.Value
                        ?? string.Empty;

        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
        {
            await Result.Failure(Error.Unauthorized("Auth.Unauthorized", "Token inválido o no provisto."))
                .ToResult()
                .ExecuteAsync(HttpContext);
            return;
        }

        var nameParts = nameClaim.Trim().Split(' ', 2);
        var firstName = nameParts.Length > 0 ? nameParts[0] : nameClaim;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var response = new MeResponse
        {
            Id = userId,
            Email = emailClaim,
            FirstName = firstName,
            LastName = lastName,
            FullName = nameClaim,
            Roles = [new RoleResponseDto { Id = roleClaim, Name = roleClaim }],
            CatStatusAccountId = 1
        };

        await Result<MeResponse>.Success(response).ToResult().ExecuteAsync(HttpContext);
    }
}
