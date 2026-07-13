using Estapar.ParkingManager.Application.Interfaces;

namespace Estapar.ParkingManager.Tests.Application.Fakes;

/// <summary>Implementação sem efeito (no-op) de IUnitOfWork usada pelos testes da camada Application; rastreia quantas vezes foi chamada.</summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
