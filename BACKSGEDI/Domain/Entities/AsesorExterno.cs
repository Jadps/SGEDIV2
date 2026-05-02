using BACKSGEDI.Domain.Interfaces;

namespace BACKSGEDI.Domain.Entities;

public class AsesorExterno : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public string Puesto { get; set; } = string.Empty;
    public string TelefonoOficina { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
}
