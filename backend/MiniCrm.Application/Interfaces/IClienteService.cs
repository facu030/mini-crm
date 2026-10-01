using MiniCrm.Application.Dtos;

namespace MiniCrm.Application.Interfaces;

public interface IClienteService
{
    Task<List<ClienteDto>> ListarAsync(ClienteFiltroDto filtro);
    Task<ClienteDto> ObtenerPorIdAsync(int id);
    Task<ClienteDto> CrearAsync(ClienteRequest request);
    Task<ClienteDto> EditarAsync(int id, ClienteRequest request);
}
