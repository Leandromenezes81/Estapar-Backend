using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

/// <summary>Implementação de <see cref="ISpotRepository"/> baseada no EF Core.</summary>
public sealed class SpotRepository : ISpotRepository
{
    private readonly AppDbContext _dbContext;

    public SpotRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Busca uma vaga pelo seu Id.</summary>
    public Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Spots.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    /// <summary>Busca a vaga de um setor pelas suas coordenadas geográficas.</summary>
    public Task<Spot?> GetByCoordinatesAsync(int sectorId, double lat, double lng, CancellationToken cancellationToken = default) =>
        _dbContext.Spots.FirstOrDefaultAsync(s => s.SectorId == sectorId && s.Lat == lat && s.Lng == lng, cancellationToken);

    /// <summary>Adiciona uma nova vaga ao contexto (persistida apenas após <see cref="IUnitOfWork.SaveChangesAsync"/>).</summary>
    public Task AddAsync(Spot spot, CancellationToken cancellationToken = default)
    {
        _dbContext.Spots.Add(spot);
        return Task.CompletedTask;
    }
}
