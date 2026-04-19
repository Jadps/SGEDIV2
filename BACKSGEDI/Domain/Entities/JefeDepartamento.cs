using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class JefeDepartamento : ISoftDelete, IActivatable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int CarreraId { get; set; }
    public CatCarrera Carrera { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

