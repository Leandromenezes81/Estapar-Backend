using Estapar.Garage.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Infrastructure.Persistence;

/// <summary>Contexto do EF Core para a Estapar.Garage.Api, com os conjuntos de garagens, setores e vagas.</summary>
public sealed class GarageDbContext : DbContext
{
    public DbSet<GarageEntity> Garages => Set<GarageEntity>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Spot> Spots => Set<Spot>();

    public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options)
    {
    }

    /// <summary>Aplica todas as configurações de entidade (<see cref="IEntityTypeConfiguration{TEntity}"/>) definidas no assembly.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GarageDbContext).Assembly);
    }
}
