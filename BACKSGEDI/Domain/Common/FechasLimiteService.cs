using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKSGEDI.Domain.Common;

public class FechasLimiteService : IFechasLimiteService
{
    private readonly ApplicationDbContext _db;

    public FechasLimiteService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DateTime> GetFechaLimiteAsync(TipoAcuerdo tipoAcuerdo, int carreraId, string semestre, CancellationToken ct = default)
    {
        var config = await _db.ConfiguracionesFechasLimites
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TipoAcuerdo == tipoAcuerdo && c.CarreraId == carreraId && c.Semestre == semestre, ct);

        if (config != null)
        {
            return config.FechaLimite;
        }

        return CalculateDefault(tipoAcuerdo, semestre);
    }

    public DateTime CalculateDefault(TipoAcuerdo tipoAcuerdo, string semestre)
    {
        var parts = semestre.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var periodo))
        {
            return DateTime.UtcNow.AddMonths(1);
        }

        var endMonth = periodo == 1 ? 6 : 12;
        var endDay = periodo == 1 ? 30 : 31;

        return new DateTime(year, endMonth, endDay, 23, 59, 59, DateTimeKind.Utc);
    }
}