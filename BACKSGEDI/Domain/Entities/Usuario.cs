using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class Usuario : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<UsuarioRol> Roles { get; set; } = new List<UsuarioRol>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public int Status { get; set; } = (int)EntityStatus.Activo;
    public Alumno? Alumno { get; set; }
    public Coordinador? Coordinador { get; set; }
    public JefeDepartamento? JefeDepartamento { get; set; }
    public Profesor? Profesor { get; set; }
    public AsesorInterno? AsesorInterno { get; set; }
    public AsesorExterno? AsesorExterno { get; set; }
}
