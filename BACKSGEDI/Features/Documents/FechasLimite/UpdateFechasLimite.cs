using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Documents.FechasLimite;

public record UpdateFechasLimiteRequest
{
    public int CarreraId { get; init; }
    public string Semestre { get; init; } = string.Empty;
    public List<FechaLimiteUpdateDto> Fechas { get; init; } = [];
}

public record FechaLimiteUpdateDto
{
    public TipoAcuerdo TipoAcuerdo { get; init; }
    public DateTime FechaLimite { get; init; }
}

public class UpdateFechasLimite : Endpoint<UpdateFechasLimiteRequest>
{
    private readonly ApplicationDbContext _db;

    public UpdateFechasLimite(ApplicationDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put("/api/fechas-limite");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(UpdateFechasLimiteRequest req, CancellationToken ct)
    {
        if (!User.GetRoles().Contains(SystemRoles.Admin))
        {
            var userId = User.GetUserId();
            var userInfo = await _db.Usuarios
                .Include(u => u.Coordinador)
                .Include(u => u.JefeDepartamento)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            int allowedCarreraId = 0;
            if (userInfo?.Coordinador != null)
                allowedCarreraId = userInfo.Coordinador.CarreraId;
            else if (userInfo?.JefeDepartamento != null)
                allowedCarreraId = userInfo.JefeDepartamento.CarreraId;

            if (req.CarreraId != allowedCarreraId)
            {
                await Result.Failure(Error.Forbidden("FechasLimite.Forbidden", "No tienes permisos para modificar fechas límite de otra carrera")).ToResult().ExecuteAsync(HttpContext);
                return;
            }
        }

        var semestre = string.IsNullOrEmpty(req.Semestre) ? SemestreHelper.GetSemestreActual() : req.Semestre;

        var existing = await _db.ConfiguracionesFechasLimites
            .Where(c => c.CarreraId == req.CarreraId && c.Semestre == semestre)
            .ToListAsync(ct);

        foreach (var update in req.Fechas)
        {
            var target = existing.FirstOrDefault(e => e.TipoAcuerdo == update.TipoAcuerdo);
            if (target != null)
            {
                target.FechaLimite = update.FechaLimite.ToUniversalTime();
            }
            else
            {
                await _db.ConfiguracionesFechasLimites.AddAsync(new ConfiguracionFechaLimite
                {
                    CarreraId = req.CarreraId,
                    Semestre = semestre,
                    TipoAcuerdo = update.TipoAcuerdo,
                    FechaLimite = update.FechaLimite.ToUniversalTime()
                }, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        await Result.Success().ToResult().ExecuteAsync(HttpContext);
    }
}
