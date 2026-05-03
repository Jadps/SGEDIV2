using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class Materia : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Creditos { get; set; }
    public int Semestre { get; set; }
    public int CarreraId { get; set; }
    public CatCarrera? Carrera { get; set; }
    public int Status { get; set; } = (int)EntityStatus.Activo;
}
