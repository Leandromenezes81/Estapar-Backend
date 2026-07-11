using Estapar.Application.Interfaces;
using Estapar.Domain.Entities;
using Estapar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.Infrastructure.Repositories;

public sealed class SectorRepository : ISectorRepository
{
    private readonly AppDbContext _dbContext;

    public SectorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
