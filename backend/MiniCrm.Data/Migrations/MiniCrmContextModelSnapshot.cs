using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiniCrm.Data;

#nullable disable

namespace MiniCrm.Data.Migrations
{
    [DbContext(typeof(MiniCrmContext))]
    partial class MiniCrmContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.31");

            modelBuilder.Entity("MiniCrm.Domain.Entities.Cliente", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asesor")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cuit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<int>("Estado")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("FechaActualizacion")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("FechaCreacion")
                        .HasColumnType("TEXT");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly?>("ProximoContacto")
                        .HasColumnType("TEXT");

                    b.Property<string>("Telefono")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Cuit")
                        .IsUnique();

                    b.ToTable("Clientes", (string)null);
                });

            modelBuilder.Entity("MiniCrm.Domain.Entities.Gestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClienteId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comentario")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("EstadoResultante")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("FechaGestion")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly?>("ProximoContacto")
                        .HasColumnType("TEXT");

                    b.Property<int>("TipoContacto")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ClienteId");

                    b.ToTable("Gestiones", (string)null);
                });

            modelBuilder.Entity("MiniCrm.Domain.Entities.Gestion", b =>
                {
                    b.HasOne("MiniCrm.Domain.Entities.Cliente", "Cliente")
                        .WithMany("Gestiones")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cliente");
                });

            modelBuilder.Entity("MiniCrm.Domain.Entities.Cliente", b =>
                {
                    b.Navigation("Gestiones");
                });
#pragma warning restore 612, 618
        }
    }
}
