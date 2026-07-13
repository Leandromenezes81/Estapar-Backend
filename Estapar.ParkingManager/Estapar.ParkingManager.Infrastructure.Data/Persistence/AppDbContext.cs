using Estapar.ParkingManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Persistence;

/// <summary>Contexto do EF Core para a Estapar.ParkingManager.Api, com os conjuntos de setores, vagas e sessões de estacionamento.</summary>
public sealed class AppDbContext : DbContext
{
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Spot> Spots => Set<Spot>();
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>Aplica todas as configurações de entidade (<see cref="IEntityTypeConfiguration{TEntity}"/>) definidas no assembly.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
