using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Exceptions;

namespace Estapar.ParkingManager.Application.UseCases.Parked;

/// <summary>
/// Handles a PARKED event: resolves the physical spot from lat/lng,
/// marks it occupied, and links it to the vehicle's open session. The
/// price factor was already locked in at ENTRY.
/// </summary>
public sealed class HandleParkedUseCase
{
    private readonly IParkingSessionRepository _sessions;
    private readonly ISpotRepository _spots;
    private readonly IUnitOfWork _unitOfWork;

    public HandleParkedUseCase(IParkingSessionRepository sessions, ISpotRepository spots, IUnitOfWork unitOfWork)
    {
        _sessions = sessions;
        _spots = spots;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ParkedCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetOpenByLicensePlateAsync(command.LicensePlate, cancellationToken)
            ?? throw new SessionNotFoundException(command.LicensePlate);

        var spot = await _spots.GetByCoordinatesAsync(command.SectorId, command.Lat, command.Lng, cancellationToken)
            ?? throw new InvalidOperationException($"No spot found in sector '{command.SectorId}' at coordinates ({command.Lat}, {command.Lng}).");

        spot.Occupy();
        session.AssignSpot(spot.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
