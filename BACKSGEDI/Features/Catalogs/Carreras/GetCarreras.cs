using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Extensions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Features.Catalogs.Carreras;

public record CarreraDto(int Id, string Name);

public class GetCarrerasEndpoint : EndpointWithoutRequest<List<CarreraDto>>
{
    private readonly ApplicationDbContext _db;

    public GetCarrerasEndpoint(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/catalogs/carreras");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var carreras = await _db.Carreras
            .Where(c => !c.IsDeleted)
            .Select(c => new CarreraDto(c.Id, c.Nombre))
            .ToListAsync(ct);

        await Result<List<CarreraDto>>.Success(carreras).ToResult().ExecuteAsync(HttpContext);
    }
}
