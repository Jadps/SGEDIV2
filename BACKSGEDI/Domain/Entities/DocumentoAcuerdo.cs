using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class DocumentoAcuerdo : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public Guid? ProfesorId { get; set; }
    public Usuario? Profesor { get; set; }

    public Guid? AsesorInternoId { get; set; }
    public Usuario? AsesorInterno { get; set; }

    public Guid? AsesorExternoId { get; set; }
    public Usuario? AsesorExterno { get; set; }

    public TipoAcuerdo TipoAcuerdo { get; set; }
    public string Semestre { get; set; } = string.Empty;
    
    public string? RutaArchivo { get; set; }
    public DateTime? FechaSubida { get; set; }
    public DateTime FechaLimite { get; set; }
    
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.PendienteRevision;
    public string? MotivoRechazo { get; set; }
    
    public int Version { get; set; } = 1;
    public bool EsVersionActual { get; set; } = true;

    public Guid SubidoPorUsuarioId { get; set; }
    public Guid? RevisadoPorUsuarioId { get; set; }
    public DateTime? FechaRevision { get; set; }
    public bool IsDeleted { get; set; } = false;
}

