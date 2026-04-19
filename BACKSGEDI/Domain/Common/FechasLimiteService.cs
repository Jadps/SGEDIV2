using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Common;

public static class FechasLimiteService
{
    public static DateTime GetFechaLimite(TipoAcuerdo tipo, string semestre)
    {
        var parts = semestre.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var periodo))
        {
            return DateTime.UtcNow.AddMonths(1);
        }

        var startMonth = periodo == 1 ? DocumentConstants.Semester1_StartMonth : DocumentConstants.Semester2_StartMonth;
        var inicioSemestre = new DateTime(year, startMonth, 1);

        return tipo switch
        {
            TipoAcuerdo.AnexoIII => inicioSemestre.AddDays(DocumentConstants.AnexoIII_DaysFromStart),
            TipoAcuerdo.AnexoII => inicioSemestre.AddDays(DocumentConstants.AnexoII_DaysFromStart),
            _ => inicioSemestre.AddDays(DocumentConstants.DefaultDeadlineDays)
        };
    }
}
