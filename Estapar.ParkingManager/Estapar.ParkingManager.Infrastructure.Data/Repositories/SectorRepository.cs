using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

public sealed class SectorRepository : ISectorRepository
{
    private readonly AppDbContext _dbContext;

    public SectorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Sector?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Sectors.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Sector?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _dbContext.Sectors.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Sectors.ToListAsync(cancellationToken);

    public Task AddAsync(Sector sector, CancellationToken cancellationToken = default)
    {
        _dbContext.Sectors.Add(sector);
        return Task.CompletedTask;
    }
}
