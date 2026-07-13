using Estapar.Garage.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Infrastructure.Persistence;

public sealed class GarageDbContext : DbContext
{
    public DbSet<GarageEntity> Garages => Set<GarageEntity>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Spot> Spots => Set<Spot>();

    public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GarageDbContext).Assembly);
    }
}
