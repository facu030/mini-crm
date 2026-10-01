using Microsoft.AspNetCore.Mvc;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Interfaces;

namespace MiniCrm.Api.Controllers;

[ApiController]
[Route("api/clientes/{clienteId:int}/gestiones")]
public class GestionesController : ControllerBase
{
    private readonly IGestionService _service;

    public GestionesController(IGestionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<GestionDto>>> Listar(int clienteId)
    {
        return Ok(await _service.ListarAsync(clienteId));
    }

    [HttpPost]
    public async Task<ActionResult<GestionDto>> Crear(int clienteId, GestionRequest request)
    {
        var gestion = await _service.CrearAsync(clienteId, request);
        return StatusCode(StatusCodes.Status201Created, gestion);
    }
}
