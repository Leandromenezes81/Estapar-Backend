using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.Interfaces;

public interface IParkingSessionRepository
{
    Task<ParkingSession?> GetOpenByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);

    /// <summary>Number of sessions currently open (ENTRY received, EXIT not yet) for a sector.</summary>
    Task<int> CountActiveBySectorAsync(int sectorId, CancellationToken cancellationToken = default);

    Task AddAsync(ParkingSession session, CancellationToken cancellationToken = default);

    /// <summary>Sum of AmountCharged for sessions closed on the given sector and date.</summary>
    Task<decimal> SumRevenueAsync(int sectorId, DateOnly date, CancellationToken cancellationToken = default);
}
