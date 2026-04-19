using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class UsuarioRol : ISoftDelete, IActivatable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

