namespace BACKSGEDI.Domain.Entities;

public class UsuarioRol
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Role { get; set; } = string.Empty;
}
