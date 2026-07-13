using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.ValueObjects;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

public sealed class ParkingSessionRepository : IParkingSessionRepository
{
    private readonly AppDbContext _dbContext;

    public ParkingSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ParkingSession?> GetOpenByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        var plate = LicensePlate.Create(licensePlate);

        return _dbContext.ParkingSessions
            .Where(p => p.LicensePlate == plate && p.ExitTime == null)
            .OrderByDescending(p => p.EntryTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountActiveBySectorAsync(int sectorId, CancellationToken cancellationToken = default) =>
        _dbContext.ParkingSessions.CountAsync(p => p.SectorId == sectorId && p.ExitTime == null, cancellationToken);

    public Task AddAsync(ParkingSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.ParkingSessions.Add(session);
        return Task.CompletedTask;
    }

    public Task<decimal> SumRevenueAsync(int sectorId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        return _dbContext.ParkingSessions
            .Where(p => p.SectorId == sectorId
                     && p.ExitTime != null
                     && p.ExitTime >= start
                     && p.ExitTime < end)
            .SumAsync(p => p.AmountCharged!.Amount, cancellationToken);
    }
}
