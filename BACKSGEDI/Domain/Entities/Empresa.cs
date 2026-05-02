using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class Empresa : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public virtual ICollection<AsesorExterno> AsesoresExternos { get; set; } = new List<AsesorExterno>();
}
