using MiniCrm.Application.Dtos;
using MiniCrm.Application.Interfaces;
using MiniCrm.Domain.Enums;
using MiniCrm.Domain.Interfaces;

namespace MiniCrm.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IClienteRepository _repository;

    public DashboardService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardResumenDto> ObtenerResumenAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return new DashboardResumenDto
        {
            TotalClientes = await _repository.ContarAsync(),
            Prospectos = await _repository.ContarAsync(EstadoCliente.Prospecto),
            Interesados = await _repository.ContarAsync(EstadoCliente.Interesado),
            SeguimientosVencidos = await _repository.ContarSeguimientosVencidosAsync(hoy)
        };
    }
}
