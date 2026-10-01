using MiniCrm.Domain.Entities;

namespace MiniCrm.Domain.Interfaces;

public interface IGestionRepository
{
    Task<List<Gestion>> ListarAsync(int clienteId);
    Task AgregarAsync(Gestion gestion);
    Task GuardarAsync();
}
