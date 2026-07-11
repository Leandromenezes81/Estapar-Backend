using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Estapar.Infrastructure.Persistence;

/// <summary>
/// Used only by the `dotnet ef` design-time tooling (e.g. `migrations add`)
/// so migrations can be authored against the Infrastructure project without
/// needing the Api's dependency injection wired up.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EstaparDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}
