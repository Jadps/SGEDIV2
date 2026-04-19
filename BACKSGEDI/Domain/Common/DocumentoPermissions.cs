using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Common;

public static class DocumentoPermissions
{
    private static readonly Dictionary<TipoAcuerdo, string[]> UploadPermissions = new()
    {
        { TipoAcuerdo.AnexoI, new[] { SystemRoles.Alumno, SystemRoles.AsesorExterno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoII, new[] { SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoIII, new[] { SystemRoles.AsesorInterno, SystemRoles.AsesorExterno, SystemRoles.Profesor, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoIV, new[] { SystemRoles.Alumno, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoV, new[] { SystemRoles.AsesorInterno, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoVI, new[] { SystemRoles.AsesorExterno, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoVII, new[] { SystemRoles.Profesor, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoVIII, new[] { SystemRoles.Alumno, SystemRoles.Admin } }
    };

    public static bool CanUpload(TipoAcuerdo tipo, IEnumerable<string> roles)
    {
        if (!UploadPermissions.TryGetValue(tipo, out var allowedRoles)) return false;
        return roles.Any(r => allowedRoles.Contains(r));
    }

    public static bool CanView(IEnumerable<string> roles, bool isMyCareer, bool isMyStudent)
    {
        if (roles.Contains(SystemRoles.Admin)) return true;

        if ((roles.Contains(SystemRoles.Coordinador) || roles.Contains(SystemRoles.JefeDepartamento)) && isMyCareer)
            return true;

        if ((roles.Contains(SystemRoles.Profesor) || roles.Contains(SystemRoles.AsesorInterno) || roles.Contains(SystemRoles.AsesorExterno)) && isMyStudent)
            return true;

        if (roles.Contains(SystemRoles.Alumno) && isMyStudent) return true;

        return false;
    }
}
