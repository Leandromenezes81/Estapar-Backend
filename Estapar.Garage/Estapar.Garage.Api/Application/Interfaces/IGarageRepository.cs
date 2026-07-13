using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Application.Interfaces;

public interface IGarageRepository
{
    Task<GarageEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<GarageEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(GarageEntity garage, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
