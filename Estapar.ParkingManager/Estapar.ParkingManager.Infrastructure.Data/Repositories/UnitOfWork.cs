using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
