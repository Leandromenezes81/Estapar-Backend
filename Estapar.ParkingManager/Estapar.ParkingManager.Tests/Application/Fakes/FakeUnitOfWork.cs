using Estapar.ParkingManager.Application.Interfaces;

namespace Estapar.ParkingManager.Tests.Application.Fakes;

/// <summary>No-op IUnitOfWork used by Application-layer tests; tracks how many times it was called.</summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
