using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;

namespace MiniCrm.Domain.Interfaces;

public interface IClienteRepository
{
    Task<List<Cliente>> ListarAsync(string? busqueda, EstadoCliente? estado, bool descendente);
    Task<int> ContarAsync(EstadoCliente? estado = null);
    Task<int> ContarSeguimientosVencidosAsync(DateOnly hoy);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteCuitAsync(string cuit, int? excluirId = null);
    Task AgregarAsync(Cliente cliente);
    Task GuardarAsync();
}
