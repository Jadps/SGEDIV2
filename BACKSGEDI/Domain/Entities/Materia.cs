using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class Materia : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Creditos { get; set; }
    public int Semestre { get; set; }
    public int CarreraId { get; set; }
    public CatCarrera? Carrera { get; set; }
    public bool IsDeleted { get; set; } = false;
}
