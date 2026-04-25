using BACKSGEDI.Domain.Common;
using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;
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

public record GetFechasLimiteRequest
{
    public int CarreraId { get; init; }
    public string Semestre { get; init; } = string.Empty;
}

public record FechaLimiteDto
{
    public TipoAcuerdo TipoAcuerdo { get; init; }
    public DateTime FechaLimite { get; init; }
    public bool IsDefault { get; init; }
}

public class GetFechasLimite : Endpoint<GetFechasLimiteRequest, List<FechaLimiteDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly IFechasLimiteService _fechasLimiteService;

    public GetFechasLimite(ApplicationDbContext db, IFechasLimiteService fechasLimiteService)
    {
        _db = db;
        _fechasLimiteService = fechasLimiteService;
    }

    public override void Configure()
    {
        Get("/api/fechas-limite");
        Roles(SystemRoles.Admin, SystemRoles.Coordinador);
    }

    public override async Task HandleAsync(GetFechasLimiteRequest req, CancellationToken ct)
    {
        var semestre = string.IsNullOrEmpty(req.Semestre) ? SemestreHelper.GetSemestreActual() : req.Semestre;

        var customFechas = await _db.ConfiguracionesFechasLimites
            .AsNoTracking()
            .Where(c => c.CarreraId == req.CarreraId && c.Semestre == semestre)
            .ToListAsync(ct);

        var resultList = new List<FechaLimiteDto>();
        
        foreach (TipoAcuerdo tipo in Enum.GetValues(typeof(TipoAcuerdo)))
        {
            var custom = customFechas.FirstOrDefault(c => c.TipoAcuerdo == tipo);
            if (custom != null)
            {
                resultList.Add(new FechaLimiteDto
                {
                    TipoAcuerdo = tipo,
                    FechaLimite = custom.FechaLimite,
                    IsDefault = false
                });
            }
            else
            {
                resultList.Add(new FechaLimiteDto
                {
                    TipoAcuerdo = tipo,
                    FechaLimite = _fechasLimiteService.CalculateDefault(tipo, semestre),
                    IsDefault = true
                });
            }
        }

        await Result<List<FechaLimiteDto>>.Success(resultList).ToResult().ExecuteAsync(HttpContext);
    }
}
