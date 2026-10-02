using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Services;
using MiniCrm.Data;
using MiniCrm.Data.Repositories;
using MiniCrm.Data.Seeds;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;
using Xunit;

namespace MiniCrm.Tests;

public class DatosInicialesTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private MiniCrmContext _context = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MiniCrmContext>().UseSqlite(_connection).Options;
        _context = new MiniCrmContext(options);
        await _context.Database.MigrateAsync();
    }

    [Fact]
    public async Task Cargar_BaseVaciaCreaClientesYGestionesCoherentes()
    {
        await DatosIniciales.CargarAsync(_context);
        _context.ChangeTracker.Clear();

        var clientes = await _context.Clientes.Include(c => c.Gestiones).ToListAsync();
        Assert.Equal(5, clientes.Count);
        Assert.Equal(5, clientes.Select(c => c.Cuit).Distinct().Count());
        Assert.Equal(5, clientes.Select(c => c.Estado).Distinct().Count());
        Assert.Equal(5, clientes.Sum(c => c.Gestiones.Count));
        Assert.True(clientes.Count(c => c.Gestiones.Count > 0) > 1);
        Assert.Contains(clientes, c => c.Gestiones.Count > 1);
        Assert.Contains(clientes, c => c.Gestiones.Count == 0);

        foreach (var cliente in clientes.Where(c => c.Gestiones.Count > 0))
        {
            var historial = cliente.Gestiones.OrderBy(g => g.FechaGestion).ToList();
            var ultima = historial.Last();
            Assert.Equal(ultima.EstadoResultante, cliente.Estado);
            Assert.Equal(ultima.FechaGestion, cliente.FechaActualizacion);
            Assert.Equal(historial.LastOrDefault(g => g.ProximoContacto.HasValue)?.ProximoContacto,
                cliente.ProximoContacto);
            Assert.All(historial, g => Assert.True(g.FechaGestion >= cliente.FechaCreacion));
            Assert.All(historial, g => Assert.Equal(DateTimeKind.Utc, g.FechaGestion.Kind));
        }

        var resumen = await new DashboardService(new ClienteRepository(_context)).ObtenerResumenAsync();
        Assert.Equal(5, resumen.TotalClientes);
        Assert.Equal(1, resumen.Prospectos);
        Assert.Equal(1, resumen.Interesados);
        Assert.Equal(2, resumen.SeguimientosVencidos);
    }

    [Fact]
    public async Task Cargar_OtraVezNoDuplicaNiRestableceLosCambios()
    {
        await DatosIniciales.CargarAsync(_context);
        var cliente = await _context.Clientes.SingleAsync(c => c.Estado == EstadoCliente.Prospecto);
        cliente.Nombre = "Nombre editado";
        await new GestionService(new ClienteRepository(_context), new GestionRepository(_context))
            .CrearAsync(cliente.Id, new GestionRequest
            {
                TipoContacto = TipoContacto.Llamada,
                Comentario = "Gestión cargada por el usuario",
                EstadoResultante = EstadoCliente.Interesado,
                ProximoContacto = DateOnly.FromDateTime(DateTime.Today).AddDays(1)
            });
        var actualizado = cliente.FechaActualizacion;

        await DatosIniciales.CargarAsync(_context);
        _context.ChangeTracker.Clear();

        var conservado = await _context.Clientes.Include(c => c.Gestiones).SingleAsync(c => c.Id == cliente.Id);
        Assert.Equal(5, await _context.Clientes.CountAsync());
        Assert.Equal(6, await _context.Gestiones.CountAsync());
        Assert.Equal("Nombre editado", conservado.Nombre);
        Assert.Equal(EstadoCliente.Interesado, conservado.Estado);
        Assert.Equal(actualizado, conservado.FechaActualizacion);
        Assert.Single(conservado.Gestiones);
        Assert.Equal("Gestión cargada por el usuario", conservado.Gestiones.Single().Comentario);
    }

    [Fact]
    public async Task Cargar_ConUnClienteExistenteConservaLaBase()
    {
        var cliente = new Cliente { Nombre = "Cliente propio", Cuit = "30123456789" };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        var fecha = cliente.FechaActualizacion;

        await DatosIniciales.CargarAsync(_context);
        _context.ChangeTracker.Clear();

        var conservado = await _context.Clientes.SingleAsync();
        Assert.Equal(cliente.Id, conservado.Id);
        Assert.Equal("Cliente propio", conservado.Nombre);
        Assert.Equal("30123456789", conservado.Cuit);
        Assert.Equal(fecha, conservado.FechaActualizacion);
        Assert.Empty(await _context.Gestiones.ToListAsync());
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
