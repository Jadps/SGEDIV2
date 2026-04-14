namespace BACKSGEDI.Domain.Entities;

public class Coordinador
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int CarreraId { get; set; }
    public CatCarrera Carrera { get; set; } = null!;
}
