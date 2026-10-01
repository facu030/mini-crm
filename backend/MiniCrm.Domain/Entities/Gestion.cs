using MiniCrm.Domain.Enums;

namespace MiniCrm.Domain.Entities;

public class Gestion
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public TipoContacto TipoContacto { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public EstadoCliente EstadoResultante { get; set; }
    public DateTime FechaGestion { get; set; } = DateTime.UtcNow;
    public DateOnly? ProximoContacto { get; set; }

    public Cliente Cliente { get; set; } = null!;
}
