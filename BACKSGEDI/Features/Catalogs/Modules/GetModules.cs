using FastEndpoints;
using BACKSGEDI.Domain.Common;
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
        var modules = new List<ModuleDto>
        {
            new()
            {
                Id = "dashboard",
                Description = "Dashboard",
                Icon = "pi pi-home",
                Action = "/dashboard",
                Order = 1,
                ModuleTypeId = 1,
                SubModules = []
            },
            new()
            {
                Id = "alumnos",
                Description = "Alumnos",
                Icon = "pi pi-users",
                Action = null,
                Order = 2,
                ModuleTypeId = 1,
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
                        SubModules = []
                    }
                ]
            }
        };

        await Result<List<ModuleDto>>.Success(modules).ToResult().ExecuteAsync(HttpContext);
    }
}
