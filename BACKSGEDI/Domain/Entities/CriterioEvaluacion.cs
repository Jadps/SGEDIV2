using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class CriterioEvaluacion : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ContratoId { get; set; }
    public ContratoProfesor? Contrato { get; set; }
    
    public TipoCriterio Tipo { get; set; } 
    public string Detalle { get; set; } = string.Empty;
    public int Porcentaje { get; set; }

    public int Status { get; set; } = (int)EntityStatus.Activo;
}
