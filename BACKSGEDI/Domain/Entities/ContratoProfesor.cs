using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class ContratoProfesor : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid MateriaId { get; set; }
    public Materia? Materia { get; set; }
    
    public int CarreraId { get; set; }
    public CatCarrera? Carrera { get; set; }
    
    public Guid AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }
    
    public Guid ProfesorId { get; set; }
    public Profesor? Profesor { get; set; }
    
    public ModalidadContrato Modalidad { get; set; } = ModalidadContrato.Presencial;
    public string? Descripcion { get; set; }
    
    public EstadoContrato Estado { get; set; } = EstadoContrato.Pendiente;
    public string? MotivoRechazo { get; set; }
    public DateTime? FechaAceptacion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int Status { get; set; } = (int)EntityStatus.Activo;

    public ICollection<CriterioEvaluacion> Criterios { get; set; } = new List<CriterioEvaluacion>();
}
