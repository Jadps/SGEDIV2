using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class CargaAcademica : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public Guid MateriaId { get; set; }
    public Materia? Materia { get; set; }

    public Guid ProfesorId { get; set; }
    public Profesor? Profesor { get; set; }

    public string Semestre { get; set; } = string.Empty;

    public int Status { get; set; } = (int)EntityStatus.Activo;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
