using System.ComponentModel.DataAnnotations;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Exceptions;
using MiniCrm.Application.Interfaces;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Exceptions;
using MiniCrm.Domain.Interfaces;

namespace MiniCrm.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ClienteDto>> ListarAsync(ClienteFiltroDto filtro)
    {
        var errores = new Dictionary<string, string[]>();
        if (filtro.Estado.HasValue && !Enum.IsDefined(filtro.Estado.Value))
            errores["estado"] = ["El estado no es válido."];

        var orden = filtro.Orden?.Trim().ToLowerInvariant() ?? "asc";
        if (orden != "asc" && orden != "desc")
            errores["orden"] = ["El orden debe ser asc o desc."];

        if (errores.Count > 0)
            throw new DatosInvalidosException(errores);

        var clientes = await _repository.ListarAsync(
            TextoOpcional(filtro.Busqueda), filtro.Estado, orden == "desc");

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return clientes.Select(cliente => Mapear(cliente, hoy)).ToList();
    }

    public async Task<ClienteDto> ObtenerPorIdAsync(int id)
    {
        var cliente = await BuscarClienteAsync(id);
        return Mapear(cliente, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<ClienteDto> CrearAsync(ClienteRequest request)
    {
        Validar(request);
        var cuit = NormalizarCuit(request.Cuit!);

        if (await _repository.ExisteCuitAsync(cuit))
            throw new CuitDuplicadoException();

        var cliente = new Cliente();
        AplicarDatos(cliente, request, cuit);
        await _repository.AgregarAsync(cliente);
        await _repository.GuardarAsync();

        return Mapear(cliente, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<ClienteDto> EditarAsync(int id, ClienteRequest request)
    {
        var cliente = await BuscarClienteAsync(id);
        Validar(request);
        var cuit = NormalizarCuit(request.Cuit!);

        if (await _repository.ExisteCuitAsync(cuit, id))
            throw new CuitDuplicadoException();

        AplicarDatos(cliente, request, cuit);
        cliente.FechaActualizacion = DateTime.UtcNow;
        await _repository.GuardarAsync();

        return Mapear(cliente, DateOnly.FromDateTime(DateTime.Today));
    }

    private async Task<Cliente> BuscarClienteAsync(int id)
    {
        return await _repository.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException("No se encontró el cliente.");
    }

    private static void Validar(ClienteRequest request)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            errores["nombre"] = ["El nombre es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.Cuit) || NormalizarCuit(request.Cuit).Length == 0)
            errores["cuit"] = ["El CUIT es obligatorio."];
        if (!Enum.IsDefined(request.Estado))
            errores["estado"] = ["El estado no es válido."];

        var email = TextoOpcional(request.Email);
        if (email is not null && !new EmailAddressAttribute().IsValid(email))
            errores["email"] = ["El correo electrónico no tiene un formato válido."];

        if (errores.Count > 0)
            throw new DatosInvalidosException(errores);
    }

    private static void AplicarDatos(Cliente cliente, ClienteRequest request, string cuit)
    {
        cliente.Nombre = request.Nombre!.Trim();
        cliente.Cuit = cuit;
        cliente.Telefono = TextoOpcional(request.Telefono);
        cliente.Email = TextoOpcional(request.Email);
        cliente.Estado = request.Estado;
        cliente.Asesor = TextoOpcional(request.Asesor);
    }

    private static string NormalizarCuit(string cuit)
    {
        return string.Concat(cuit.Where(caracter => !char.IsWhiteSpace(caracter) && caracter != '-'));
    }

    private static string? TextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static ClienteDto Mapear(Cliente cliente, DateOnly hoy)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Cuit = cliente.Cuit,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            Estado = cliente.Estado,
            Asesor = cliente.Asesor,
            ProximoContacto = cliente.ProximoContacto,
            FechaCreacion = cliente.FechaCreacion,
            FechaActualizacion = cliente.FechaActualizacion,
            SeguimientoVencido = cliente.ProximoContacto.HasValue && cliente.ProximoContacto.Value < hoy
        };
    }
}
