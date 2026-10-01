using MiniCrm.Domain.Enums;

namespace MiniCrm.Application.Dtos;

public class ClienteFiltroDto
{
    public string? Busqueda { get; set; }
    public EstadoCliente? Estado { get; set; }
    public string? Orden { get; set; } = "asc";
}
