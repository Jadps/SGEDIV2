using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class ConfiguracionFechaLimite
{
    public int Id { get; set; }
    public TipoAcuerdo TipoAcuerdo { get; set; }
    public int CarreraId { get; set; }
    public string Semestre { get; set; } = string.Empty;
    public DateTime FechaLimite { get; set; }
}
