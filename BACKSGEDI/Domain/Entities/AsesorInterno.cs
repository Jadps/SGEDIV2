using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class AsesorInterno : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string NumeroEmpleado { get; set; } = string.Empty;
    public string Cubiculo { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
}
