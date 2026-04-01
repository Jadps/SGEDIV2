namespace BACKSGEDI.Domain.Entities;

public class Alumno
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Matricula { get; set; } = string.Empty;
    public int CarreraId { get; set; }
    public virtual ICollection<DocumentoAlumno> Documentos { get; set; } = new List<DocumentoAlumno>();
}
