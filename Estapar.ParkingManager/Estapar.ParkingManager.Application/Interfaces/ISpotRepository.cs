using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.Interfaces;

public interface ISpotRepository
{
    Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Spot?> GetByCoordinatesAsync(int sectorId, double lat, double lng, CancellationToken cancellationToken = default);
    Task AddAsync(Spot spot, CancellationToken cancellationToken = default);
}
