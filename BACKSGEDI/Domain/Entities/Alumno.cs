using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class Alumno : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public int CarreraId { get; set; }
    public CatCarrera? Carrera { get; set; }
    public int SemestreId { get; set; }

    public Guid? EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public Guid? AsesorInternoId { get; set; }
    public AsesorInterno? AsesorInterno { get; set; }

    public Guid? AsesorExternoId { get; set; }
    public AsesorExterno? AsesorExterno { get; set; }

    public int Status { get; set; } = (int)EntityStatus.Activo;
    public virtual ICollection<DocumentoAlumno> Documentos { get; set; } = new List<DocumentoAlumno>();
}

