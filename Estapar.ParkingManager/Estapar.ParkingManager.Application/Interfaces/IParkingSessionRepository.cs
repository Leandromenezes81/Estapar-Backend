using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.Interfaces;

/// <summary>Repositório de sessões de estacionamento.</summary>
public interface IParkingSessionRepository
{
    /// <summary>Busca a sessão em aberto de uma placa (ainda sem EXIT registrado).</summary>
    Task<ParkingSession?> GetOpenByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);

    /// <summary>Número de sessões atualmente em aberto (ENTRY recebido, EXIT ainda não) em um setor.</summary>
    Task<int> CountActiveBySectorAsync(int sectorId, CancellationToken cancellationToken = default);

    /// <summary>Adiciona uma nova sessão de estacionamento.</summary>
    Task AddAsync(ParkingSession session, CancellationToken cancellationToken = default);

    /// <summary>Soma o valor cobrado (AmountCharged) das sessões encerradas em um setor e data específicos.</summary>
    Task<decimal> SumRevenueAsync(int sectorId, DateOnly date, CancellationToken cancellationToken = default);
}
