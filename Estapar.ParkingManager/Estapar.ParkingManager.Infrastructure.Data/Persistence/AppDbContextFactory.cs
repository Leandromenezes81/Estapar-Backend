using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Estapar.ParkingManager.Infrastructure.Data.Persistence;

/// <summary>
/// Usada apenas pelas ferramentas de design-time do `dotnet ef` (ex.:
/// `migrations add`), para que migrations possam ser criadas a partir do
/// projeto Infrastructure sem depender da injeção de dependência da Api.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>Cria um <see cref="AppDbContext"/> apontando para uma instância local do SQL Server, usado apenas em tempo de design.</summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EstaparParkingManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}
