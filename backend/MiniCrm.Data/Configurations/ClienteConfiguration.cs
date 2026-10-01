using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Domain.Entities;

namespace MiniCrm.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Nombre).IsRequired();
        builder.Property(cliente => cliente.Cuit).IsRequired();
        builder.HasIndex(cliente => cliente.Cuit).IsUnique();

        // SQLite no conserva la marca UTC de DateTime al leer las fechas.
        builder.Property(cliente => cliente.FechaCreacion)
            .HasConversion(fecha => fecha, fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
        builder.Property(cliente => cliente.FechaActualizacion)
            .HasConversion(fecha => fecha, fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
    }
}
