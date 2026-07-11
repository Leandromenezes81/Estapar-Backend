using Estapar.Application.Interfaces;
using Estapar.Domain.Entities;
using Estapar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.Infrastructure.Repositories;

public sealed class SpotRepository : ISpotRepository
{
    private readonly AppDbContext _dbContext;

    public SpotRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Spots.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Spot?> GetByCoordinatesAsync(double lat, double lng, CancellationToken cancellationToken = default) =>
        _dbContext.Spots.FirstOrDefaultAsync(s => s.Lat == lat && s.Lng == lng, cancellationToken);

    public Task AddAsync(Spot spot, CancellationToken cancellationToken = default)
    {
        _dbContext.Spots.Add(spot);
        return Task.CompletedTask;
    }
}
