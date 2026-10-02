using MiniCrm.Application.Dtos;
using MiniCrm.Application.Exceptions;
using MiniCrm.Application.Interfaces;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Interfaces;

namespace MiniCrm.Application.Services;

public class GestionService : IGestionService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IGestionRepository _gestionRepository;

    public GestionService(IClienteRepository clienteRepository, IGestionRepository gestionRepository)
    {
        _clienteRepository = clienteRepository;
        _gestionRepository = gestionRepository;
    }

    public async Task<List<GestionDto>> ListarAsync(int clienteId)
    {
        await BuscarClienteAsync(clienteId);
        var gestiones = await _gestionRepository.ListarAsync(clienteId);
        return gestiones.Select(Mapear).ToList();
    }

    public async Task<GestionDto> CrearAsync(int clienteId, GestionRequest request)
    {
        var cliente = await BuscarClienteAsync(clienteId);
        Validar(request);

        var fecha = DateTime.UtcNow;
        var gestion = new Gestion
        {
            ClienteId = clienteId,
            TipoContacto = request.TipoContacto,
            Comentario = request.Comentario!.Trim(),
            EstadoResultante = request.EstadoResultante,
            FechaGestion = fecha,
            ProximoContacto = request.ProximoContacto
        };

        cliente.Estado = request.EstadoResultante;
        cliente.FechaActualizacion = fecha;
        if (request.ProximoContacto.HasValue)
            cliente.ProximoContacto = request.ProximoContacto;

        await _gestionRepository.AgregarAsync(gestion);

        await _gestionRepository.GuardarAsync();

        return Mapear(gestion);
    }

    private async Task<Cliente> BuscarClienteAsync(int clienteId)
    {
        return await _clienteRepository.ObtenerPorIdAsync(clienteId)
            ?? throw new RecursoNoEncontradoException("No se encontró el cliente.");
    }

    private static void Validar(GestionRequest request)
    {
        var errores = new Dictionary<string, string[]>();

        if (!Enum.IsDefined(request.TipoContacto))
            errores["tipoContacto"] = ["El tipo de contacto no es válido."];
        if (string.IsNullOrWhiteSpace(request.Comentario))
            errores["comentario"] = ["El comentario es obligatorio."];
        if (!Enum.IsDefined(request.EstadoResultante))
            errores["estadoResultante"] = ["El estado resultante no es válido."];

        if (errores.Count > 0)
            throw new DatosInvalidosException(errores);
    }

    private static GestionDto Mapear(Gestion gestion)
    {
        return new GestionDto
        {
            Id = gestion.Id,
            ClienteId = gestion.ClienteId,
            TipoContacto = gestion.TipoContacto,
            Comentario = gestion.Comentario,
            EstadoResultante = gestion.EstadoResultante,
            FechaGestion = gestion.FechaGestion,
            ProximoContacto = gestion.ProximoContacto
        };
    }
}
