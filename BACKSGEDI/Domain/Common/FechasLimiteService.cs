using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Common;

public static class FechasLimiteService
{
    public static DateTime GetDefaultFechaLimite(string semestre)
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