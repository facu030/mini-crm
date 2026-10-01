using Microsoft.AspNetCore.Mvc;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Interfaces;

namespace MiniCrm.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> Listar([FromQuery] ClienteFiltroDto filtro)
    {
        return Ok(await _service.ListarAsync(filtro));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id)
    {
        return Ok(await _service.ObtenerPorIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteRequest request)
    {
        var cliente = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Editar(int id, ClienteRequest request)
    {
        return Ok(await _service.EditarAsync(id, request));
    }
}
