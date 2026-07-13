using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.ValueObjects;

namespace Estapar.ParkingManager.Tests.Application.Fakes;

/// <summary>Implementação em memória de IParkingSessionRepository, usada pelos testes da camada Application em vez de uma biblioteca de mocking.</summary>
public sealed class FakeParkingSessionRepository : IParkingSessionRepository
{
    private readonly List<ParkingSession> _sessions;

    public FakeParkingSessionRepository(params ParkingSession[] seed) => _sessions = new List<ParkingSession>(seed);

    public IReadOnlyList<ParkingSession> Sessions => _sessions;

    public Task<ParkingSession?> GetOpenByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        var plate = LicensePlate.Create(licensePlate);
        return Task.FromResult(_sessions.FirstOrDefault(s => s.LicensePlate.Equals(plate) && s.IsOpen));
    }

    public Task<int> CountActiveBySectorAsync(int sectorId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessions.Count(s => s.SectorId == sectorId && s.IsOpen));

    public Task AddAsync(ParkingSession session, CancellationToken cancellationToken = default)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }

    public Task<decimal> SumRevenueAsync(int sectorId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var total = _sessions
            .Where(s => s.SectorId == sectorId && s.ExitTime is not null && s.ExitTime >= start && s.ExitTime < end)
            .Sum(s => s.AmountCharged?.Amount ?? 0m);

        return Task.FromResult(total);
    }
}
