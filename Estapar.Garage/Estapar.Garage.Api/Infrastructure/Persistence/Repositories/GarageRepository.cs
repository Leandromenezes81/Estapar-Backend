using Estapar.Garage.Api.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Infrastructure.Persistence.Repositories;

/// <summary>Implementação de <see cref="IGarageRepository"/> baseada no EF Core.</summary>
public sealed class GarageRepository : IGarageRepository
{
    private readonly GarageDbContext _dbContext;

    public GarageRepository(GarageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Busca uma garagem pelo Id, incluindo seus setores e vagas.</summary>
    public Task<GarageEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Garages
            .Include(g => g.Sectors)
            .Include(g => g.Spots)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    /// <summary>Retorna todas as garagens, incluindo seus setores e vagas.</summary>
    public Task<List<GarageEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Garages
            .Include(g => g.Sectors)
            .Include(g => g.Spots)
            .ToListAsync(cancellationToken);

    /// <summary>Adiciona uma nova garagem ao contexto (persistida apenas após <see cref="SaveChangesAsync"/>).</summary>
    public async Task AddAsync(GarageEntity garage, CancellationToken cancellationToken = default) =>
        await _dbContext.Garages.AddAsync(garage, cancellationToken);

    /// <summary>Persiste todas as alterações pendentes no contexto do EF Core.</summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
