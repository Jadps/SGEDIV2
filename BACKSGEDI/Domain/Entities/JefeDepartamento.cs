using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class JefeDepartamento : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int CarreraId { get; set; }
    public CatCarrera Carrera { get; set; } = null!;
    public int Status { get; set; } = (int)EntityStatus.Activo;
}

