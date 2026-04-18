namespace BACKSGEDI.Domain.Common;

public static class StatusHelper
{
    public static string GetText(bool isActive) =>
        isActive ? "Activo" : "Cuenta Inactiva";

    public static string GetSeverity(bool isActive, bool isMyCareer) =>
        (!isActive && isMyCareer) ? "danger" :
        (!isActive)               ? "secondary" :
                                    "success";
}
