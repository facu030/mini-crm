using MiniCrm.Application.Dtos;

namespace MiniCrm.Application.Interfaces;

public interface IGestionService
{
    Task<List<GestionDto>> ListarAsync(int clienteId);
    Task<GestionDto> CrearAsync(int clienteId, GestionRequest request);
}
