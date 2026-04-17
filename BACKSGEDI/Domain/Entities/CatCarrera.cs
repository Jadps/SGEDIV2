using System.ComponentModel.DataAnnotations.Schema;

namespace BACKSGEDI.Domain.Entities;

[Table("Carreras")]
public class CatCarrera
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
}
