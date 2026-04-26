using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class PlantillaDocumento : ISoftDelete
{
    public int Id { get; set; }
    public TipoPlantilla TipoDocumento { get; set; }
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
    public Guid SubidaPorUsuarioId { get; set; }
    public bool EsVigente { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}

