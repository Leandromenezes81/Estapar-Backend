using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.ValueObjects;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

/// <summary>Implementação de <see cref="IParkingSessionRepository"/> baseada no EF Core.</summary>
public sealed class ParkingSessionRepository : IParkingSessionRepository
{
    private readonly AppDbContext _dbContext;

    public ParkingSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Busca a sessão em aberto mais recente de uma placa (ainda sem EXIT registrado).</summary>
    public Task<ParkingSession?> GetOpenByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        var plate = LicensePlate.Create(licensePlate);

        return _dbContext.ParkingSessions
            .Where(p => p.LicensePlate == plate && p.ExitTime == null)
            .OrderByDescending(p => p.EntryTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Conta as sessões atualmente em aberto (ENTRY recebido, EXIT ainda não) em um setor.</summary>
    public Task<int> CountActiveBySectorAsync(int sectorId, CancellationToken cancellationToken = default) =>
        _dbContext.ParkingSessions.CountAsync(p => p.SectorId == sectorId && p.ExitTime == null, cancellationToken);

    /// <summary>Adiciona uma nova sessão de estacionamento ao contexto (persistida apenas após <see cref="IUnitOfWork.SaveChangesAsync"/>).</summary>
    public Task AddAsync(ParkingSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.ParkingSessions.Add(session);
        return Task.CompletedTask;
    }

    /// <summary>Soma o valor cobrado (AmountCharged) das sessões encerradas no setor e data informados.</summary>
    public Task<decimal> SumRevenueAsync(int sectorId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        return _dbContext.ParkingSessions
            .Where(p => p.SectorId == sectorId
                     && p.ExitTime != null
                     && p.ExitTime >= start
                     && p.ExitTime < end)
            .SumAsync(p => p.AmountCharged!.Amount, cancellationToken);
    }
}
