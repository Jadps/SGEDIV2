using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class PlantillaDocumento
{
    public int Id { get; set; }
    public TipoPlantilla TipoDocumento { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
    public Guid SubidaPorUsuarioId { get; set; }
    public bool EsVigente { get; set; } = true;
}
