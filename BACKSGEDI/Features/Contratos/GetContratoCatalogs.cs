using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;

namespace BACKSGEDI.Features.Contratos;

public record ContratoCatalogsResponse
{
    public List<CatalogItemDto> Modalidades { get; init; } = new();
    public List<CatalogItemDto> TiposCriterio { get; init; } = new();
    public List<CatalogItemDto> Estados { get; init; } = new();
}

public record CatalogItemDto
{
    public int Value { get; init; }
    public string Label { get; init; } = string.Empty;
}

public class GetContratoCatalogs : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/contratos/catalogs");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var modalidades = Enum.GetValues<ModalidadContrato>()
            .Select(v => new CatalogItemDto { Value = (int)v, Label = v.ToString() })
            .ToList();

        var tipos = Enum.GetValues<TipoCriterio>()
            .Select(v => new CatalogItemDto { Value = (int)v, Label = v.ToString() })
            .ToList();

        var estados = Enum.GetValues<EstadoContrato>()
            .Select(v => new CatalogItemDto { Value = (int)v, Label = v.ToString() })
            .ToList();

        var response = new ContratoCatalogsResponse
        {
            Modalidades = modalidades,
            TiposCriterio = tipos,
            Estados = estados
        };

        await Result<ContratoCatalogsResponse>.Success(response).ToResult().ExecuteAsync(HttpContext);
    }
}
