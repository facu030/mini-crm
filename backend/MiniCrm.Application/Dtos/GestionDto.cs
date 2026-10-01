using MiniCrm.Domain.Enums;

namespace MiniCrm.Application.Dtos;

public class GestionDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public TipoContacto TipoContacto { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public EstadoCliente EstadoResultante { get; set; }
    public DateTime FechaGestion { get; set; }
    public DateOnly? ProximoContacto { get; set; }
}
