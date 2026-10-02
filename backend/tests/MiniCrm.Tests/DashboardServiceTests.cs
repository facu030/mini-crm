using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Services;
using MiniCrm.Data;
using MiniCrm.Data.Repositories;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;
using Xunit;

namespace MiniCrm.Tests;

public class DashboardServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private MiniCrmContext _context = null!;
    private DashboardService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MiniCrmContext>().UseSqlite(_connection).Options;
        _context = new MiniCrmContext(options);
        await _context.Database.MigrateAsync();
        _service = new DashboardService(new ClienteRepository(_context));
    }

    [Fact]
    public async Task ObtenerResumen_SinClientesDevuelveTodosLosContadoresEnCero()
    {
        var resumen = await _service.ObtenerResumenAsync();

        Assert.Equal(0, resumen.TotalClientes);
        Assert.Equal(0, resumen.Prospectos);
        Assert.Equal(0, resumen.Interesados);
        Assert.Equal(0, resumen.SeguimientosVencidos);
    }

    [Fact]
    public async Task ObtenerResumen_CuentaEstadosYSoloContactosAnterioresAHoy()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        _context.Clientes.AddRange(
            Cliente("1", EstadoCliente.Prospecto, hoy.AddDays(-1)),
            Cliente("2", EstadoCliente.Prospecto, hoy),
            Cliente("3", EstadoCliente.Interesado, hoy.AddDays(1)),
            Cliente("4", EstadoCliente.Interesado, null),
            Cliente("5", EstadoCliente.Contactado, hoy.AddDays(-2)),
            Cliente("6", EstadoCliente.NoInteresado, null),
            Cliente("7", EstadoCliente.Cliente, hoy));
        await _context.SaveChangesAsync();

        var resumen = await _service.ObtenerResumenAsync();

        Assert.Equal(7, resumen.TotalClientes);
        Assert.Equal(2, resumen.Prospectos);
        Assert.Equal(2, resumen.Interesados);
        Assert.Equal(2, resumen.SeguimientosVencidos);
        var listado = await new ClienteService(new ClienteRepository(_context)).ListarAsync(new ClienteFiltroDto());
        Assert.Equal(listado.Count(c => c.SeguimientoVencido), resumen.SeguimientosVencidos);
    }

    [Fact]
    public async Task ObtenerResumen_ReflejaCambiosDeGestionesSinContarDosVecesAlCliente()
    {
        var cliente = Cliente("1", EstadoCliente.Prospecto, null);
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        var inicial = await _service.ObtenerResumenAsync();
        Assert.Equal(1, inicial.Prospectos);
        Assert.Equal(0, inicial.Interesados);
        Assert.Equal(0, inicial.SeguimientosVencidos);

        var gestiones = new GestionService(new ClienteRepository(_context), new GestionRepository(_context));
        await gestiones.CrearAsync(cliente.Id, new GestionRequest
        {
            TipoContacto = TipoContacto.Llamada, Comentario = "Mostró interés",
            EstadoResultante = EstadoCliente.Interesado,
            ProximoContacto = DateOnly.FromDateTime(DateTime.Today).AddDays(-1)
        });
        await gestiones.CrearAsync(cliente.Id, new GestionRequest
        {
            TipoContacto = TipoContacto.WhatsApp, Comentario = "Segundo contacto",
            EstadoResultante = EstadoCliente.Interesado
        });

        var actualizado = await _service.ObtenerResumenAsync();

        Assert.Equal(1, actualizado.TotalClientes);
        Assert.Equal(0, actualizado.Prospectos);
        Assert.Equal(1, actualizado.Interesados);
        Assert.Equal(1, actualizado.SeguimientosVencidos);
        Assert.Equal(2, await _context.Gestiones.CountAsync());
    }

    private static Cliente Cliente(string cuit, EstadoCliente estado, DateOnly? proximoContacto)
    {
        return new Cliente { Nombre = "Comercio " + cuit, Cuit = cuit, Estado = estado, ProximoContacto = proximoContacto };
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
