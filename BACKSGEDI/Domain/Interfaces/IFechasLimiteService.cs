using BACKSGEDI.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace BACKSGEDI.Domain.Interfaces;

public interface IFechasLimiteService
{
    Task<DateTime> GetFechaLimiteAsync(TipoAcuerdo tipoAcuerdo, int carreraId, string semestre, CancellationToken ct = default);
    DateTime CalculateDefault(TipoAcuerdo tipoAcuerdo, string semestre);
}
