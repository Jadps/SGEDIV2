using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class Alumno : ISoftDelete, IActivatable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public int CarreraId { get; set; }
    public int SemestreId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public virtual ICollection<DocumentoAlumno> Documentos { get; set; } = new List<DocumentoAlumno>();
}

