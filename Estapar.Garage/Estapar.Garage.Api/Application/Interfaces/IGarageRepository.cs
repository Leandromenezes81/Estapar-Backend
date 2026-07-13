using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Application.Interfaces;

/// <summary>Repositório de garagens.</summary>
public interface IGarageRepository
{
    /// <summary>Busca uma garagem (com seus setores e vagas) pelo Id.</summary>
    Task<GarageEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Retorna todas as garagens cadastradas (com seus setores e vagas).</summary>
    Task<List<GarageEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Adiciona uma nova garagem.</summary>
    Task AddAsync(GarageEntity garage, CancellationToken cancellationToken = default);

    /// <summary>Persiste todas as alterações pendentes.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
