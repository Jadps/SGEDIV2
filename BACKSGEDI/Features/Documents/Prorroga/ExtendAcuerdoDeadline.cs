using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.Acuerdos;

public record ExtendDeadlineRequest
{
    public Guid AcuerdoId { get; set; }
    public DateTime NuevaFechaLimite { get; set; }
}

public class ExtendAcuerdoDeadline : Endpoint<ExtendDeadlineRequest>
{
    private readonly ApplicationDbContext _db;

    public ExtendAcuerdoDeadline(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/api/acuerdos/{acuerdoId}/prorroga");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(ExtendDeadlineRequest req, CancellationToken ct)
    {
        var acuerdo = await _db.DocumentosAcuerdos.FirstOrDefaultAsync(a => a.Id == req.AcuerdoId, ct);

        if (acuerdo is null)
        {
            await Result.Failure(Error.NotFound("Acuerdo.NotFound", "El acuerdo no existe."))
                .ToResult().ExecuteAsync(HttpContext);
            return;
        }

        acuerdo.FechaLimite = DateTime.SpecifyKind(req.NuevaFechaLimite, DateTimeKind.Utc);
        
        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
