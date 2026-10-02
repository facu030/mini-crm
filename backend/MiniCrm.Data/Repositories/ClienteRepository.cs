using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;
using MiniCrm.Domain.Exceptions;
using MiniCrm.Domain.Interfaces;

namespace MiniCrm.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly MiniCrmContext _context;

    public ClienteRepository(MiniCrmContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> ListarAsync(string? busqueda, EstadoCliente? estado, bool descendente)
    {
        var query = _context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.ToLowerInvariant();
            var cuit = string.Concat(busqueda.Where(caracter => !char.IsWhiteSpace(caracter) && caracter != '-'));
            query = query.Where(cliente =>
                cliente.Nombre.ToLower().Contains(texto) ||
                (cuit != "" && cliente.Cuit.Contains(cuit)) ||
                (cliente.Telefono != null && cliente.Telefono.Contains(busqueda)));
        }

        if (estado.HasValue)
            query = query.Where(cliente => cliente.Estado == estado.Value);

        // los clientes sin próximo contacto quedan al final en ambos órdenes
        var ordenada = query.OrderBy(cliente => cliente.ProximoContacto == null);
        return descendente
            ? await ordenada.ThenByDescending(cliente => cliente.ProximoContacto).ThenBy(cliente => cliente.Id).ToListAsync()
            : await ordenada.ThenBy(cliente => cliente.ProximoContacto).ThenBy(cliente => cliente.Id).ToListAsync();
    }

    public async Task<int> ContarAsync(EstadoCliente? estado = null)
    {
        var query = _context.Clientes.AsNoTracking();
        if (estado.HasValue)
            query = query.Where(cliente => cliente.Estado == estado.Value);

        return await query.CountAsync();
    }

    public async Task<int> ContarSeguimientosVencidosAsync(DateOnly hoy)
    {
        return await _context.Clientes.CountAsync(cliente =>
            cliente.ProximoContacto.HasValue && cliente.ProximoContacto.Value < hoy);
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<bool> ExisteCuitAsync(string cuit, int? excluirId = null)
    {
        return await _context.Clientes.AnyAsync(cliente =>
            cliente.Cuit == cuit && (!excluirId.HasValue || cliente.Id != excluirId.Value));
    }

    public async Task AgregarAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
    }

    public async Task GuardarAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            throw new CuitDuplicadoException();
        }
    }
}
