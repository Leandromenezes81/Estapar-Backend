using Estapar.Domain.Entities;

namespace Estapar.Application.Interfaces;

public interface ISectorRepository
{
    Task<Sector?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Sector sector, CancellationToken cancellationToken = default);
}
