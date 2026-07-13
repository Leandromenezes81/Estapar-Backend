using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estapar.ParkingManager.Infrastructure.Data.Repositories;

/// <summary>Implementação de <see cref="ISectorRepository"/> baseada no EF Core.</summary>
public sealed class SectorRepository : ISectorRepository
{
    private readonly AppDbContext _dbContext;

    public SectorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Busca um setor pelo seu Id.</summary>
    public Task<Sector?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Sectors.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    /// <summary>Busca um setor pelo nome.</summary>
    public Task<Sector?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _dbContext.Sectors.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);

    /// <summary>Retorna todos os setores cadastrados.</summary>
    public async Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Sectors.ToListAsync(cancellationToken);

    /// <summary>Adiciona um novo setor ao contexto (persistido apenas após <see cref="IUnitOfWork.SaveChangesAsync"/>).</summary>
    public Task AddAsync(Sector sector, CancellationToken cancellationToken = default)
    {
        _dbContext.Sectors.Add(sector);
        return Task.CompletedTask;
    }
}
