using Estapar.Garage.Api.Application.Interfaces;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Tests.Application.Fakes;

/// <summary>Implementação em memória de IGarageRepository, usada pelos testes da camada Application em vez de uma biblioteca de mocking.</summary>
public sealed class FakeGarageRepository : IGarageRepository
{
    private readonly List<GarageEntity> _garages;

    public FakeGarageRepository(params GarageEntity[] seed) => _garages = new List<GarageEntity>(seed);

    public int SaveChangesCallCount { get; private set; }

    public Task<GarageEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_garages.FirstOrDefault(g => g.Id == id));

    public Task<List<GarageEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_garages.ToList());

    public Task AddAsync(GarageEntity garage, CancellationToken cancellationToken = default)
    {
        _garages.Add(garage);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
