namespace BACKSGEDI.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Alumno";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Alumno? Alumno { get; set; }
}
