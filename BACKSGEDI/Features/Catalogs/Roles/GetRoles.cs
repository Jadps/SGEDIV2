using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;

namespace BACKSGEDI.Features.Catalogs.Roles;

public record RoleDto(string Label, string Value);

public class GetRolesEndpoint : EndpointWithoutRequest<List<RoleDto>>
{
    public override void Configure()
    {
        Get("/api/catalogs/roles");
        Roles(SystemRoles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roles = new List<RoleDto>
        {
            new("Administrador", SystemRoles.Admin),
            new("Profesor", SystemRoles.Profesor),
            new("Coordinador", SystemRoles.Coordinador),
            new("Asesor Interno", SystemRoles.AsesorInterno),
            new("Jefe de Departamento", SystemRoles.JefeDepartamento)
        };

        await Result<List<RoleDto>>.Success(roles).ToResult().ExecuteAsync(HttpContext);
    }
}
