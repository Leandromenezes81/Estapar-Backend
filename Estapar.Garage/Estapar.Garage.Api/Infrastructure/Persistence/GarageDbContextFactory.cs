using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Estapar.Garage.Api.Infrastructure.Persistence;

/// <summary>
/// Usada apenas pelas ferramentas de design-time do `dotnet ef` (ex.: `migrations add`)
/// para que migrations possam ser criadas sem precisar de um banco de dados ativo ou
/// da injeção de dependência da aplicação.
/// </summary>
public sealed class GarageDbContextFactory : IDesignTimeDbContextFactory<GarageDbContext>
{
    /// <summary>Cria um <see cref="GarageDbContext"/> apontando para uma instância local do SQL Server, usado apenas em tempo de design.</summary>
    public GarageDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GarageDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EstaparGarageDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new GarageDbContext(optionsBuilder.Options);
    }
}
