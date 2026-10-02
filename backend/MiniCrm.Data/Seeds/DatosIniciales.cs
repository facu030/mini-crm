using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Enums;

namespace MiniCrm.Data.Seeds;

public static class DatosIniciales
{
    public static async Task CargarAsync(MiniCrmContext context)
    {
        if (await context.Clientes.AnyAsync())
            return;

        var ahora = DateTime.UtcNow;
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var clientes = new[]
        {
            new Cliente
            {
                Nombre = "Almacén Norte",
                Cuit = "30990000001",
                Telefono = "3815550101",
                Email = "almacen@example.com",
                Asesor = "Facundo"
            },
            new Cliente
            {
                Nombre = "Ferretería Centro",
                Cuit = "30990000002",
                Telefono = "3815550102",
                Email = "ferreteria@example.com",
                Asesor = "Facundo"
            },
            new Cliente
            {
                Nombre = "Librería Sur",
                Cuit = "30990000003",
                Telefono = "3815550103",
                Email = "libreria@example.com",
                Asesor = "María"
            },
            new Cliente
            {
                Nombre = "Tienda Oeste",
                Cuit = "30990000004",
                Telefono = "3815550104",
                Email = "tienda@example.com",
                Asesor = "María"
            },
            new Cliente
            {
                Nombre = "Panadería Este",
                Cuit = "30990000005",
                Telefono = "3815550105",
                Email = "panaderia@example.com",
                Asesor = "Facundo"
            }
        };

        foreach (var cliente in clientes)
        {
            cliente.FechaCreacion = ahora.AddDays(-7);
            cliente.FechaActualizacion = cliente.FechaCreacion;
        }

        var gestiones = new[]
        {
            new Gestion
            {
                Cliente = clientes[2],
                TipoContacto = TipoContacto.Correo,
                Comentario = "Se envió información sobre el servicio.",
                EstadoResultante = EstadoCliente.Contactado,
                FechaGestion = ahora.AddDays(-4),
                ProximoContacto = hoy.AddDays(-2)
            },
            new Gestion
            {
                Cliente = clientes[1],
                TipoContacto = TipoContacto.Llamada,
                Comentario = "Solicitó que lo contactemos nuevamente.",
                EstadoResultante = EstadoCliente.Contactado,
                FechaGestion = ahora.AddDays(-3),
                ProximoContacto = hoy.AddDays(-1)
            },
            new Gestion
            {
                Cliente = clientes[3],
                TipoContacto = TipoContacto.Otro,
                Comentario = "Indicó que no está interesado por el momento.",
                EstadoResultante = EstadoCliente.NoInteresado,
                FechaGestion = ahora.AddDays(-2)
            },
            new Gestion
            {
                Cliente = clientes[2],
                TipoContacto = TipoContacto.WhatsApp,
                Comentario = "Mostró interés y pidió una propuesta.",
                EstadoResultante = EstadoCliente.Interesado,
                FechaGestion = ahora.AddDays(-1)
            },
            new Gestion
            {
                Cliente = clientes[4],
                TipoContacto = TipoContacto.Reunion,
                Comentario = "Se acordó comenzar a trabajar con el comercio.",
                EstadoResultante = EstadoCliente.Cliente,
                FechaGestion = ahora.AddDays(-1),
                ProximoContacto = hoy.AddDays(3)
            }
        };

        foreach (var gestion in gestiones)
        {
            gestion.Cliente.Estado = gestion.EstadoResultante;
            gestion.Cliente.FechaActualizacion = gestion.FechaGestion;
            if (gestion.ProximoContacto.HasValue)
                gestion.Cliente.ProximoContacto = gestion.ProximoContacto;
        }

        context.Clientes.AddRange(clientes);
        context.Gestiones.AddRange(gestiones);
        await context.SaveChangesAsync();
    }
}
