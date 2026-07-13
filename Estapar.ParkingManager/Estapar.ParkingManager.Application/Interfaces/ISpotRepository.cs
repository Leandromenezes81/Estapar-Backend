using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.Interfaces;

/// <summary>Repositório de vagas.</summary>
public interface ISpotRepository
{
    /// <summary>Busca uma vaga pelo seu Id.</summary>
    Task<Spot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Busca a vaga de um setor pelas suas coordenadas geográficas.</summary>
    Task<Spot?> GetByCoordinatesAsync(int sectorId, double lat, double lng, CancellationToken cancellationToken = default);

    /// <summary>Adiciona uma nova vaga.</summary>
    Task AddAsync(Spot spot, CancellationToken cancellationToken = default);
}
