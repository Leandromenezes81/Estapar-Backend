using Estapar.Garage.Api.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Infrastructure.Persistence.Repositories;

public sealed class GarageRepository : IGarageRepository
{
    private readonly GarageDbContext _dbContext;

    public GarageRepository(GarageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GarageEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Garages
            .Include(g => g.Sectors)
            .Include(g => g.Spots)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<List<GarageEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Garages
            .Include(g => g.Sectors)
            .Include(g => g.Spots)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(GarageEntity garage, CancellationToken cancellationToken = default) =>
        await _dbContext.Garages.AddAsync(garage, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
