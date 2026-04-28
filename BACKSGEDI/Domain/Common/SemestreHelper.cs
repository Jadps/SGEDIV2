namespace BACKSGEDI.Domain.Common;

public static class SemestreHelper
{
    public static string GetSemestreActual()
    {
        var hoy = DateTime.Now;
        var periodo = hoy.Month <= 6 ? "1" : "2";
        return $"{hoy.Year}-{periodo}";
    }
}
