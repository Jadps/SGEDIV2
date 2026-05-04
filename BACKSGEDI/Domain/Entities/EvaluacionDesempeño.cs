using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class EvaluacionDesempeño : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Status { get; set; } = (int)EntityStatus.Activo;
    public Guid? AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public Guid? EvaluadorId { get; set; }
    public Usuario? Evaluador { get; set; }

    public int Calificacion { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public string Semestre { get; set; } = string.Empty;
    public DateTime FechaEvaluacion { get; set; }
}
