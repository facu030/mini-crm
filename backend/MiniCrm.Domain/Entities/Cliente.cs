using MiniCrm.Domain.Enums;

namespace MiniCrm.Domain.Entities;

public class Cliente
{
    public Cliente()
    {
        FechaCreacion = DateTime.UtcNow;
        FechaActualizacion = FechaCreacion;
    }

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public EstadoCliente Estado { get; set; } = EstadoCliente.Prospecto;
    public string? Asesor { get; set; }
    public DateOnly? ProximoContacto { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public ICollection<Gestion> Gestiones { get; set; } = new List<Gestion>();
}
