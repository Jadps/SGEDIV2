using System.Security.Claims;
using FastEndpoints;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Catalogs.Modules;

public class SubModuleDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Action { get; set; }
    public int Order { get; set; }
    public int ModuleTypeId { get; set; }
    public string? ParentId { get; set; }
    public List<string> AllowedRoles { get; set; } = [];
    public List<SubModuleDto> SubModules { get; set; } = [];
}

public class ModuleDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Action { get; set; }
    public int Order { get; set; }
    public int ModuleTypeId { get; set; }
    public string? ParentId { get; set; }
    public List<string> AllowedRoles { get; set; } = [];
    public List<SubModuleDto> SubModules { get; set; } = [];
}

public class GetModulesEndpoint : EndpointWithoutRequest<List<ModuleDto>>
{
    public override void Configure()
    {
        Get("/api/catalogs/modules");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roleClaims = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var allModules = GetDefaultModules();

        if (roleClaims.Any(r => r.Contains(SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
        {
            await Result<List<ModuleDto>>.Success(allModules).ToResult().ExecuteAsync(HttpContext);
            return;
        }

        var filteredModules = FilterModules(allModules, roleClaims);

        await Result<List<ModuleDto>>.Success(filteredModules).ToResult().ExecuteAsync(HttpContext);
    }

    private List<ModuleDto> GetDefaultModules()
    {
        return
        [
            new()
            {
                Id = "dashboard",
                Description = "Dashboard",
                Icon = "pi pi-home",
                Action = "/dashboard",
                Order = 1,
                ModuleTypeId = 1,
                AllowedRoles = [SystemRoles.Alumno, SystemRoles.Profesor, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno]
            },
            new()
            {
                Id = "alumnos",
                Description = "Alumnos",
                Icon = "pi pi-users",
                Action = null,
                Order = 2,
                ModuleTypeId = 1,
                AllowedRoles = [SystemRoles.Profesor, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno],
                SubModules =
                [
                    new SubModuleDto
                    {
                        Id = "alumnos-list",
                        Description = "Lista de Alumnos",
                        Icon = "pi pi-list",
                        Action = "/dashboard/alumnos",
                        Order = 1,
                        ModuleTypeId = 2,
                        AllowedRoles = [SystemRoles.Profesor, SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.AsesorInterno, SystemRoles.AsesorExterno]
                    }
                ]
            }
        ];
    }

    private List<ModuleDto> FilterModules(List<ModuleDto> modules, List<string> roles)
    {
        var filtered = new List<ModuleDto>();

        foreach (var m in modules)
        {
            if (m.AllowedRoles.Count == 0 || m.AllowedRoles.Intersect(roles, StringComparer.OrdinalIgnoreCase).Any())
            {
                var mClone = new ModuleDto
                {
                    Id = m.Id,
                    Description = m.Description,
                    Icon = m.Icon,
                    Action = m.Action,
                    Order = m.Order,
                    ModuleTypeId = m.ModuleTypeId,
                    ParentId = m.ParentId,
                    AllowedRoles = m.AllowedRoles,
                    SubModules = FilterSubModules(m.SubModules, roles)
                };
                filtered.Add(mClone);
            }
        }

        return filtered;
    }

    private List<SubModuleDto> FilterSubModules(List<SubModuleDto> subModules, List<string> roles)
    {
        var filtered = new List<SubModuleDto>();

        foreach (var sm in subModules)
        {
            if (sm.AllowedRoles.Count == 0 || sm.AllowedRoles.Intersect(roles, StringComparer.OrdinalIgnoreCase).Any())
            {
                var smClone = new SubModuleDto
                {
                    Id = sm.Id,
                    Description = sm.Description,
                    Icon = sm.Icon,
                    Action = sm.Action,
                    Order = sm.Order,
                    ModuleTypeId = sm.ModuleTypeId,
                    ParentId = sm.ParentId,
                    AllowedRoles = sm.AllowedRoles,
                    SubModules = FilterSubModules(sm.SubModules, roles)
                };
                filtered.Add(smClone);
            }
        }

        return filtered;
    }
}
