using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Dtos;
using MiniCrm.Application.Exceptions;
using MiniCrm.Application.Services;
using MiniCrm.Data;
using MiniCrm.Data.Repositories;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;
using MiniCrm.Domain.Exceptions;
using Xunit;

namespace MiniCrm.Tests;

public class ClienteServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private MiniCrmContext _context = null!;
    private ClienteService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MiniCrmContext>().UseSqlite(_connection).Options;
        _context = new MiniCrmContext(options);
        await _context.Database.MigrateAsync();
        _service = new ClienteService(new ClienteRepository(_context));
    }

    [Fact]
    public async Task Crear_RechazaCuitDuplicadoAunqueCambieElFormato()
    {
        await _service.CrearAsync(Datos("30-12345678-9"));

        await Assert.ThrowsAsync<CuitDuplicadoException>(() => _service.CrearAsync(Datos(" 30123456789 ")));

        Assert.Equal(1, await _context.Clientes.CountAsync());
    }

    [Fact]
    public async Task Editar_PermiteConservarElPropioCuit()
    {
        var cliente = await _service.CrearAsync(Datos("30123456789"));
        var datos = Datos("30-12345678-9");
        datos.Nombre = " Comercio actualizado ";
        datos.Email = " ";

        var actualizado = await _service.EditarAsync(cliente.Id, datos);

        Assert.Equal("Comercio actualizado", actualizado.Nombre);
        Assert.Equal("30123456789", actualizado.Cuit);
        Assert.Null(actualizado.Email);
        Assert.Equal(cliente.FechaCreacion, actualizado.FechaCreacion);
        Assert.Equal(DateTimeKind.Utc, actualizado.FechaActualizacion.Kind);
    }

    [Fact]
    public async Task Editar_RechazaElCuitDeOtroCliente()
    {
        var primero = await _service.CrearAsync(Datos("30123456789"));
        var segundo = await _service.CrearAsync(Datos("30111111111"));

        await Assert.ThrowsAsync<CuitDuplicadoException>(() =>
            _service.EditarAsync(segundo.Id, Datos(primero.Cuit)));

        var conservado = await _service.ObtenerPorIdAsync(segundo.Id);
        Assert.Equal(segundo.Cuit, conservado.Cuit);
    }

    [Fact]
    public async Task Editar_ConservaHistorialYProximoContacto()
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today).AddDays(3);
        var cliente = new Cliente
        {
            Nombre = "Original", Cuit = "30123456789", ProximoContacto = fecha,
            Gestiones = new List<Gestion>
            {
                new() { TipoContacto = TipoContacto.Llamada, Comentario = "Primera llamada", EstadoResultante = EstadoCliente.Contactado }
            }
        };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var datos = Datos(cliente.Cuit);
        datos.Estado = EstadoCliente.Interesado;
        await _service.EditarAsync(cliente.Id, datos);
        _context.ChangeTracker.Clear();

        var guardado = await _context.Clientes.Include(c => c.Gestiones).SingleAsync();
        Assert.Equal(fecha, guardado.ProximoContacto);
        Assert.Equal(EstadoCliente.Interesado, guardado.Estado);
        Assert.Equal("Primera llamada", Assert.Single(guardado.Gestiones).Comentario);
        Assert.Equal(EstadoCliente.Contactado, guardado.Gestiones.Single().EstadoResultante);
    }

    [Theory]
    [InlineData(null, "30123456789", null, 1, "nombre")]
    [InlineData("Comercio", " - ", null, 1, "cuit")]
    [InlineData("Comercio", "30123456789", "correo-invalido", 1, "email")]
    [InlineData("Comercio", "30123456789", null, 99, "estado")]
    public async Task Crear_ValidaDatosAntesDeGuardar(string? nombre, string? cuit, string? email, int estado, string campo)
    {
        var datos = new ClienteRequest { Nombre = nombre, Cuit = cuit, Email = email, Estado = (EstadoCliente)estado };

        var error = await Assert.ThrowsAsync<DatosInvalidosException>(() => _service.CrearAsync(datos));

        Assert.Contains(campo, error.Errores.Keys);
        Assert.Equal(0, await _context.Clientes.CountAsync());
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(null, false)]
    public async Task Listar_SoloMarcaVencidoUnContactoAnteriorAHoy(int? dias, bool vencido)
    {
        _context.Clientes.Add(new Cliente
        {
            Nombre = "Comercio", Cuit = "30123456789",
            ProximoContacto = dias.HasValue ? DateOnly.FromDateTime(DateTime.Today).AddDays(dias.Value) : null
        });
        await _context.SaveChangesAsync();

        var cliente = Assert.Single(await _service.ListarAsync(new ClienteFiltroDto()));

        Assert.Equal(vencido, cliente.SeguimientoVencido);
    }

    [Fact]
    public async Task Listar_BuscaFiltraYOrdenaConFechasNulasAlFinal()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var anterior = new Cliente { Nombre = "Comercio Centro", Cuit = "30123456789", Estado = EstadoCliente.Prospecto, ProximoContacto = hoy.AddDays(-1) };
        var futuro = new Cliente { Nombre = "Comercio Norte", Cuit = "30111111111", Telefono = "3815559876", Estado = EstadoCliente.Interesado, ProximoContacto = hoy.AddDays(2) };
        var sinFecha = new Cliente { Nombre = "Comercio Sur", Cuit = "30222222222", Estado = EstadoCliente.Cliente };
        _context.Clientes.AddRange(sinFecha, futuro, anterior);
        await _context.SaveChangesAsync();

        Assert.Equal(anterior.Id, Assert.Single(await _service.ListarAsync(new ClienteFiltroDto { Busqueda = "centro" })).Id);
        Assert.Equal(anterior.Id, Assert.Single(await _service.ListarAsync(new ClienteFiltroDto { Busqueda = "30-12345678-9" })).Id);
        Assert.Equal(futuro.Id, Assert.Single(await _service.ListarAsync(new ClienteFiltroDto { Busqueda = "5559876" })).Id);
        Assert.Equal(futuro.Id, Assert.Single(await _service.ListarAsync(new ClienteFiltroDto { Estado = EstadoCliente.Interesado })).Id);

        var ascendente = await _service.ListarAsync(new ClienteFiltroDto());
        var descendente = await _service.ListarAsync(new ClienteFiltroDto { Orden = "desc" });
        Assert.Equal(new[] { anterior.Id, futuro.Id, sinFecha.Id }, ascendente.Select(c => c.Id));
        Assert.Equal(new[] { futuro.Id, anterior.Id, sinFecha.Id }, descendente.Select(c => c.Id));
        Assert.Empty(await _service.ListarAsync(new ClienteFiltroDto { Busqueda = "sin coincidencias" }));
    }

    [Fact]
    public async Task ConsultarYEditar_InformanSiElClienteNoExiste()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.ObtenerPorIdAsync(123));
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.EditarAsync(123, Datos("30123456789")));
    }

    [Theory]
    [InlineData(99, "asc")]
    [InlineData(null, "otro")]
    public async Task Listar_RechazaEstadoUOrdenInvalidos(int? estado, string orden)
    {
        var filtro = new ClienteFiltroDto { Estado = estado.HasValue ? (EstadoCliente)estado.Value : null, Orden = orden };

        await Assert.ThrowsAsync<DatosInvalidosException>(() => _service.ListarAsync(filtro));
    }

    [Fact]
    public async Task Repositorio_ElIndiceUnicoTambienRechazaElCuitDuplicado()
    {
        await _service.CrearAsync(Datos("30123456789"));
        var repository = new ClienteRepository(_context);
        await repository.AgregarAsync(new Cliente { Nombre = "Duplicado", Cuit = "30123456789" });

        await Assert.ThrowsAsync<CuitDuplicadoException>(() => repository.GuardarAsync());
        Assert.Equal(1, await _context.Clientes.CountAsync());
    }

    private static ClienteRequest Datos(string cuit)
    {
        return new ClienteRequest { Nombre = "Comercio", Cuit = cuit, Estado = EstadoCliente.Prospecto };
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
