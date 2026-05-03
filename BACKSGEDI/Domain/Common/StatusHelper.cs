using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Common;

public static class StatusHelper
{
    public static string GetText(int status) => status switch
    {
        (int)EntityStatus.Activo => "Activo",
        (int)EntityStatus.SinActivar => "Sin Activar",
        (int)EntityStatus.Borrado => "Borrado",
        _ => "Desconocido"
    };

    public static string GetSeverity(int status, bool isMyCareer) => status switch
    {
        (int)EntityStatus.Activo => "success",
        (int)EntityStatus.SinActivar when isMyCareer => "warn",
        (int)EntityStatus.SinActivar => "secondary",
        (int)EntityStatus.Borrado => "danger",
        _ => "info"
    };
}
