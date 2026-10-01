using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Exceptions;
using MiniCrm.Application.Services;
using MiniCrm.Data;
using MiniCrm.Data.Repositories;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;
using Xunit;

namespace MiniCrm.Tests;

public class GestionServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private MiniCrmContext _context = null!;
    private GestionService _service = null!;
    private Cliente _cliente = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MiniCrmContext>().UseSqlite(_connection).Options;
        _context = new MiniCrmContext(options);
        await _context.Database.MigrateAsync();
        _service = new GestionService(new ClienteRepository(_context), new GestionRepository(_context));
        _cliente = new Cliente
        {
            Nombre = "Comercio", Cuit = "30123456789", Estado = EstadoCliente.Prospecto,
            ProximoContacto = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
            FechaActualizacion = DateTime.UtcNow.AddDays(-1)
        };
        _context.Clientes.Add(_cliente);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Crear_GuardaGestionYActualizaCliente()
    {
        var datos = Datos();
        datos.Comentario = " Primera llamada ";
        datos.ProximoContacto = DateOnly.FromDateTime(DateTime.Today).AddDays(3);

        var gestion = await _service.CrearAsync(_cliente.Id, datos);
        _context.ChangeTracker.Clear();

        var cliente = await _context.Clientes.SingleAsync();
        var guardada = await _context.Gestiones.SingleAsync();
        Assert.True(gestion.Id > 0);
        Assert.Equal(_cliente.Id, guardada.ClienteId);
        Assert.Equal("Primera llamada", guardada.Comentario);
        Assert.Equal(datos.TipoContacto, guardada.TipoContacto);
        Assert.Equal(datos.EstadoResultante, cliente.Estado);
        Assert.Equal(datos.ProximoContacto, cliente.ProximoContacto);
        Assert.Equal(guardada.FechaGestion, cliente.FechaActualizacion);
        Assert.Equal(DateTimeKind.Utc, guardada.FechaGestion.Kind);
        Assert.Equal(_cliente.FechaCreacion, cliente.FechaCreacion);
    }

    [Fact]
    public async Task Crear_SinProximoContactoConservaFechaEHistorialPrevios()
    {
        var primera = await _service.CrearAsync(_cliente.Id, Datos());
        var datos = Datos();
        datos.Comentario = "Segunda llamada";
        datos.EstadoResultante = EstadoCliente.Interesado;

        var segunda = await _service.CrearAsync(_cliente.Id, datos);
        _context.ChangeTracker.Clear();

        var cliente = await _context.Clientes.SingleAsync();
        var historial = await _service.ListarAsync(_cliente.Id);
        Assert.Equal(_cliente.ProximoContacto, cliente.ProximoContacto);
        Assert.Equal(EstadoCliente.Interesado, cliente.Estado);
        Assert.Null(segunda.ProximoContacto);
        Assert.Equal(new[] { segunda.Id, primera.Id }, historial.Select(g => g.Id));
        Assert.Equal("Primera llamada", historial[1].Comentario);
        Assert.Equal(EstadoCliente.Contactado, historial[1].EstadoResultante);
    }

    [Fact]
    public async Task Listar_DevuelveSoloElHistorialDelClienteEnOrdenDescendente()
    {
        var fecha = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var antigua = new Gestion { ClienteId = _cliente.Id, Comentario = "Antigua", TipoContacto = TipoContacto.Llamada, EstadoResultante = EstadoCliente.Contactado, FechaGestion = fecha.AddDays(-1) };
        var reciente = new Gestion { ClienteId = _cliente.Id, Comentario = "Reciente", TipoContacto = TipoContacto.Correo, EstadoResultante = EstadoCliente.Interesado, FechaGestion = fecha };
        var mismaFecha = new Gestion { ClienteId = _cliente.Id, Comentario = "Última", TipoContacto = TipoContacto.WhatsApp, EstadoResultante = EstadoCliente.Cliente, FechaGestion = fecha };
        var otro = new Cliente { Nombre = "Otro comercio", Cuit = "30111111111" };
        otro.Gestiones.Add(new Gestion { Comentario = "Otro cliente", TipoContacto = TipoContacto.Otro, EstadoResultante = EstadoCliente.Prospecto, FechaGestion = fecha.AddDays(1) });
        _context.Clientes.Add(otro);
        _context.Gestiones.AddRange(antigua, reciente, mismaFecha);
        await _context.SaveChangesAsync();

        var historial = await _service.ListarAsync(_cliente.Id);

        Assert.Equal(new[] { mismaFecha.Id, reciente.Id, antigua.Id }, historial.Select(g => g.Id));
        Assert.All(historial, gestion => Assert.Equal(_cliente.Id, gestion.ClienteId));
        Assert.All(historial, gestion => Assert.Equal(DateTimeKind.Utc, gestion.FechaGestion.Kind));
    }

    [Theory]
    [InlineData(0, 1, "Llamada", "tipoContacto")]
    [InlineData(99, 1, "Llamada", "tipoContacto")]
    [InlineData(1, 0, "Llamada", "estadoResultante")]
    [InlineData(1, 99, "Llamada", "estadoResultante")]
    [InlineData(1, 1, null, "comentario")]
    [InlineData(1, 1, " ", "comentario")]
    public async Task Crear_RechazaDatosInvalidosSinModificarCliente(int tipo, int estado, string? comentario, string campo)
    {
        var datos = new GestionRequest { TipoContacto = (TipoContacto)tipo, EstadoResultante = (EstadoCliente)estado, Comentario = comentario };

        var error = await Assert.ThrowsAsync<DatosInvalidosException>(() => _service.CrearAsync(_cliente.Id, datos));
        _context.ChangeTracker.Clear();

        Assert.Contains(campo, error.Errores.Keys);
        Assert.Equal(0, await _context.Gestiones.CountAsync());
        Assert.Equal(_cliente.Estado, (await _context.Clientes.SingleAsync()).Estado);
    }

    [Fact]
    public async Task ClienteInexistente_DevuelveErrorYClienteSinGestionesDevuelveListaVacia()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.CrearAsync(9999, Datos()));
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.ListarAsync(9999));
        Assert.Empty(await _service.ListarAsync(_cliente.Id));
        Assert.Equal(0, await _context.Gestiones.CountAsync());
    }

    [Fact]
    public async Task Crear_SiFallaElGuardadoNoPersisteNingunoDeLosCambios()
    {
        // Solo en esta prueba: SQLite rechaza la inserción para comprobar el rollback.
        await _context.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER RechazarGestion BEFORE INSERT ON Gestiones BEGIN SELECT RAISE(ABORT, 'Fallo de prueba'); END;");
        var datos = Datos();
        datos.ProximoContacto = DateOnly.FromDateTime(DateTime.Today).AddDays(5);

        await Assert.ThrowsAsync<DbUpdateException>(() => _service.CrearAsync(_cliente.Id, datos));
        _context.ChangeTracker.Clear();

        var cliente = await _context.Clientes.SingleAsync();
        Assert.Equal(0, await _context.Gestiones.CountAsync());
        Assert.Equal(_cliente.Estado, cliente.Estado);
        Assert.Equal(_cliente.ProximoContacto, cliente.ProximoContacto);
        Assert.Equal(_cliente.FechaActualizacion, cliente.FechaActualizacion);
    }

    private static GestionRequest Datos()
    {
        return new GestionRequest { TipoContacto = TipoContacto.Llamada, Comentario = "Primera llamada", EstadoResultante = EstadoCliente.Contactado };
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
