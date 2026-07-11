using Estapar.Domain.Entities;

namespace Estapar.Application.Interfaces;

public interface ISpotRepository
{
    Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Spot?> GetByCoordinatesAsync(double lat, double lng, CancellationToken cancellationToken = default);
    Task AddAsync(Spot spot, CancellationToken cancellationToken = default);
}
