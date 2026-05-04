using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Common;

public static class DocumentoPermissions
{
    private static readonly Dictionary<TipoAcuerdo, string[]> UploadPermissions = new()
    {
        { TipoAcuerdo.AnexoI, new[] { SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoII, new[] { SystemRoles.Coordinador, SystemRoles.JefeDepartamento, SystemRoles.Admin } },
        { TipoAcuerdo.AnexoIII, new[] { SystemRoles.Profesor, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoIV, new[] { SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoV, new[] { SystemRoles.AsesorInterno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoVI, new[] { SystemRoles.AsesorExterno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoVII, new[] { SystemRoles.Profesor, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoAcuerdo.AnexoVIII, new[] { SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador } }
    };

    private static readonly Dictionary<TipoDocumentoAlumno, string[]> StudentUploadPermissions = new()
    {
        { TipoDocumentoAlumno.Horario, new[] { SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador } },
        { TipoDocumentoAlumno.Kardex, new[] { SystemRoles.Alumno, SystemRoles.Admin, SystemRoles.Coordinador } }
    };

    private static readonly HashSet<TipoAcuerdo> MultiInstanceAcuerdos = new()
    {
        TipoAcuerdo.AnexoIII,
        TipoAcuerdo.AnexoVII
    };

    public static bool CanUpload(TipoAcuerdo tipo, IEnumerable<string> roles)
    {
        if (!UploadPermissions.TryGetValue(tipo, out var allowedRoles)) return false;
        return roles.Any(r => allowedRoles.Contains(r));
    }

    public static bool IsMultiInstance(TipoAcuerdo tipo)
    {
        return MultiInstanceAcuerdos.Contains(tipo);
    }

    public static bool CanUpload(TipoDocumentoAlumno tipo, IEnumerable<string> roles)
    {
        if (!StudentUploadPermissions.TryGetValue(tipo, out var allowedRoles)) return false;
        return roles.Any(r => allowedRoles.Contains(r));
    }
}
