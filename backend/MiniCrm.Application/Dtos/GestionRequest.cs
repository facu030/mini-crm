using MiniCrm.Domain.Enums;

namespace MiniCrm.Application.Dtos;

public class GestionRequest
{
    public TipoContacto TipoContacto { get; set; }
    public string? Comentario { get; set; }
    public EstadoCliente EstadoResultante { get; set; }
    public DateOnly? ProximoContacto { get; set; }
}
