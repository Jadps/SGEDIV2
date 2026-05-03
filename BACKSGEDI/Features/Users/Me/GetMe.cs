using System.Security.Claims;
using FastEndpoints;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Users.Me;

public record MeResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public int Status { get; set; } = 0;
}

public class GetMeEndpoint : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/api/users/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var nameClaim = User.GetName();
        var emailClaim = User.GetEmail();
        var roleClaims = User.GetRoles();

        if (userId == Guid.Empty)
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
            Roles = roleClaims,
            Status = User.GetStatus()
        };

        await Result<MeResponse>.Success(response).ToResult().ExecuteAsync(HttpContext);
    }
}

