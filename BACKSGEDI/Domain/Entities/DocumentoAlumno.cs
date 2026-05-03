using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class DocumentoAlumno
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public TipoDocumentoAlumno TipoDocumento { get; set; }
    
    public string Semestre { get; set; } = string.Empty;
    
    public string RutaArchivo { get; set; } = string.Empty;
    
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public int Version { get; set; } = 1;
    public bool EsVersionActual { get; set; } = true;
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.PendienteRevision;
    public string? MotivoRechazo { get; set; }
    public Guid? RevisadoPorUsuarioId { get; set; }
    public DateTime? FechaRevision { get; set; }
    public Guid SubidoPorUsuarioId { get; set; }
}

