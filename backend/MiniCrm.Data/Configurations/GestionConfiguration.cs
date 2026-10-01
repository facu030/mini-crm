using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Domain.Entities;

namespace MiniCrm.Data.Configurations;

public class GestionConfiguration : IEntityTypeConfiguration<Gestion>
{
    public void Configure(EntityTypeBuilder<Gestion> builder)
    {
        builder.ToTable("Gestiones");
        builder.HasKey(gestion => gestion.Id);
        builder.Property(gestion => gestion.Comentario).IsRequired();
        builder.Property(gestion => gestion.FechaGestion)
            .HasConversion(fecha => fecha, fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

        builder.HasOne(gestion => gestion.Cliente)
            .WithMany(cliente => cliente.Gestiones)
            .HasForeignKey(gestion => gestion.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
