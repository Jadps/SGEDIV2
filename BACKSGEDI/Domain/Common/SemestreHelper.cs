namespace BACKSGEDI.Domain.Common;

public static class SemestreHelper
{
    public static string GetSemestreActual()
    {
        var hoy = DateTime.Now;
        var periodo = hoy.Month <= 7 ? "1" : "2";
        return $"{hoy.Year}-{periodo}";
    }
}
