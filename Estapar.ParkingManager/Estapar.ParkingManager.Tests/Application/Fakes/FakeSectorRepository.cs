using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Tests.Application.Fakes;

/// <summary>Implementação em memória de ISectorRepository, usada pelos testes da camada Application em vez de uma biblioteca de mocking.</summary>
public sealed class FakeSectorRepository : ISectorRepository
{
    private readonly List<Sector> _sectors;

    public FakeSectorRepository(params Sector[] seed) => _sectors = new List<Sector>(seed);

    public IReadOnlyList<Sector> Sectors => _sectors;

    public Task<Sector?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sectors.FirstOrDefault(s => s.Id == id));

    public Task<Sector?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sectors.FirstOrDefault(s => s.Name == name));

    public Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Sector>>(_sectors);

    public Task AddAsync(Sector sector, CancellationToken cancellationToken = default)
    {
        _sectors.Add(sector);
        return Task.CompletedTask;
    }
}
