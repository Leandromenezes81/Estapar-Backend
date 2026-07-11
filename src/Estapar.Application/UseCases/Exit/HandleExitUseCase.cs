using Estapar.Application.Interfaces;
using Estapar.Domain.Exceptions;

namespace Estapar.Application.UseCases.Exit;

/// <summary>
/// Handles an EXIT event: closes the session (charging the fee using the
/// locked-in price factor and the sector's base price) and releases the spot.
/// </summary>
public sealed class HandleExitUseCase
{
    private readonly IParkingSessionRepository _sessions;
    private readonly ISectorRepository _sectors;
    private readonly ISpotRepository _spots;
    private readonly IUnitOfWork _unitOfWork;

    public HandleExitUseCase(
        IParkingSessionRepository sessions,
        ISectorRepository sectors,
        ISpotRepository spots,
        IUnitOfWork unitOfWork)
    {
        _sessions = sessions;
        _sectors = sectors;
        _spots = spots;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ExitCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetOpenByLicensePlateAsync(command.LicensePlate, cancellationToken)
            ?? throw new SessionNotFoundException(command.LicensePlate);

        var sector = await _sectors.GetByNameAsync(session.SectorName, cancellationToken)
            ?? throw new InvalidOperationException($"Sector '{session.SectorName}' not found.");

        session.Close(command.ExitTime, sector.BasePrice);

        if (session.SpotId is int spotId)
        {
            var spot = await _spots.GetByIdAsync(spotId, cancellationToken);
            spot?.Release();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
