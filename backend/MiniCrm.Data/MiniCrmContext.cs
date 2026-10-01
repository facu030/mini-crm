using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Entities;

namespace MiniCrm.Data;

public class MiniCrmContext : DbContext
{
    public MiniCrmContext(DbContextOptions<MiniCrmContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Gestion> Gestiones { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniCrmContext).Assembly);
    }
}
