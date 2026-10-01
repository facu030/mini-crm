using MiniCrm.Domain.Enums;

namespace MiniCrm.Application.Dtos;

public class ClienteRequest
{
    public string? Nombre { get; set; }
    public string? Cuit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public EstadoCliente Estado { get; set; } = EstadoCliente.Prospecto;
    public string? Asesor { get; set; }
}
