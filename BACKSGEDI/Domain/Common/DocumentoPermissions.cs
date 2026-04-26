using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Common;

public static class DocumentoPermissions
{
    private static readonly Dictionary<TipoAcuerdo, string[]> UploadPermissions = new()
    {
        { TipoAcuerdo.AnexoI, new[] { SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoII, new[] { SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoIII, new[] { SystemRoles.Profesor, SystemRoles.Admin } },
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
}
