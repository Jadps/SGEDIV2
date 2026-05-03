using BACKSGEDI.Domain.Interfaces;
using BACKSGEDI.Domain.Enums;

namespace BACKSGEDI.Domain.Entities;

public class AsesorExterno : IHasStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public string Puesto { get; set; } = string.Empty;
    public string TelefonoOficina { get; set; } = string.Empty;
    public int Status { get; set; } = (int)EntityStatus.Activo;
}
