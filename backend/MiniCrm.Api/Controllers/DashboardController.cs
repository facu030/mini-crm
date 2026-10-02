using Microsoft.AspNetCore.Mvc;
using MiniCrm.Application.Interfaces;
using MiniCrm.Application.Dtos;

namespace MiniCrm.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardResumenDto>> ObtenerResumen()
    {
        return Ok(await _service.ObtenerResumenAsync());
    }
}
