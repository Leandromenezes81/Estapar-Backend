using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

public sealed class SpotRepository : ISpotRepository
{
    private readonly AppDbContext _dbContext;

    public SpotRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Spots.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Spot?> GetByCoordinatesAsync(int sectorId, double lat, double lng, CancellationToken cancellationToken = default) =>
        _dbContext.Spots.FirstOrDefaultAsync(s => s.SectorId == sectorId && s.Lat == lat && s.Lng == lng, cancellationToken);

    public Task AddAsync(Spot spot, CancellationToken cancellationToken = default)
    {
        _dbContext.Spots.Add(spot);
        return Task.CompletedTask;
    }
}
