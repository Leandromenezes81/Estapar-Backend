using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Estapar.Garage.Api.Infrastructure.Persistence;

/// <summary>
/// Used only by the `dotnet ef` design-time tooling (e.g. `migrations add`) so
/// migrations can be authored without needing a live database or the app's DI.
/// </summary>
public sealed class GarageDbContextFactory : IDesignTimeDbContextFactory<GarageDbContext>
{
    public GarageDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GarageDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EstaparGarageDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new GarageDbContext(optionsBuilder.Options);
    }
}
