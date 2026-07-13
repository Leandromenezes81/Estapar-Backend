using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Tests.Application.Fakes;

/// <summary>In-memory ISpotRepository used by Application-layer tests instead of a mocking library.</summary>
public sealed class FakeSpotRepository : ISpotRepository
{
    private readonly List<Spot> _spots;

    public FakeSpotRepository(params Spot[] seed) => _spots = new List<Spot>(seed);

    public IReadOnlyList<Spot> Spots => _spots;

    public Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_spots.FirstOrDefault(s => s.Id == id));

    public Task<Spot?> GetByCoordinatesAsync(int sectorId, double lat, double lng, CancellationToken cancellationToken = default) =>
        Task.FromResult(_spots.FirstOrDefault(s => s.SectorId == sectorId && s.Lat == lat && s.Lng == lng));

    public Task AddAsync(Spot spot, CancellationToken cancellationToken = default)
    {
        _spots.Add(spot);
        return Task.CompletedTask;
    }
}
