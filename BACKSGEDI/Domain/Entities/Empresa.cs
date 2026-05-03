using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class Empresa : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int Status { get; set; } = (int)EntityStatus.Activo;
    public virtual ICollection<AsesorExterno> AsesoresExternos { get; set; } = new List<AsesorExterno>();
}
