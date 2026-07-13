using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.Interfaces;

public interface ISectorRepository
{
    Task<Sector?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Sector?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Sector sector, CancellationToken cancellationToken = default);
}
