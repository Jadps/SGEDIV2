using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class DocumentoAlumno
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;

    public TipoDocumento TipoDocumento { get; set; }
    
    public string Semestre { get; set; } = string.Empty;
    
    public string RutaArchivo { get; set; } = string.Empty;
    
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
