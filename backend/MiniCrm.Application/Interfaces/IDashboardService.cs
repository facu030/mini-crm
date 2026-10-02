using MiniCrm.Application.Dtos;

namespace MiniCrm.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenAsync();
}
