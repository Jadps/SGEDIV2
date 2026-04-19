using System.ComponentModel.DataAnnotations.Schema;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

[Table("Carreras")]
public class CatCarrera : ISoftDelete
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
}
