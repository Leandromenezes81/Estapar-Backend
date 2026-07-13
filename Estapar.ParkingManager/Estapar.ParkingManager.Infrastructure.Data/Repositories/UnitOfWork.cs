using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

/// <summary>Implementação de <see cref="IUnitOfWork"/> baseada no <see cref="AppDbContext"/> do EF Core.</summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Persiste todas as alterações pendentes no contexto do EF Core.</summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
