using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.Interfaces;

/// <summary>Repositório de setores.</summary>
public interface ISectorRepository
{
    /// <summary>Busca um setor pelo seu Id.</summary>
    Task<Sector?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Busca um setor pelo nome.</summary>
    Task<Sector?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Retorna todos os setores cadastrados.</summary>
    Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Adiciona um novo setor.</summary>
    Task AddAsync(Sector sector, CancellationToken cancellationToken = default);
}
