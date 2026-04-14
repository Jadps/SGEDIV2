using BACKSGEDI.Domain.Constants;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class Usuario : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = SystemRoles.Alumno;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Alumno? Alumno { get; set; }
    public Coordinador? Coordinador { get; set; }
    public JefeDepartamento? JefeDepartamento { get; set; }
}
