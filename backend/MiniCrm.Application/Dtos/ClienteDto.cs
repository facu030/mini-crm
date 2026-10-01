using MiniCrm.Domain.Enums;

namespace MiniCrm.Application.Dtos;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public EstadoCliente Estado { get; set; }
    public string? Asesor { get; set; }
    public DateOnly? ProximoContacto { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public bool SeguimientoVencido { get; set; }
}
