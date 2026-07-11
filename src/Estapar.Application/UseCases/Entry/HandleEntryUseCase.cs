using Estapar.Application.Interfaces;
using Estapar.Domain.Entities;
using Estapar.Domain.Exceptions;
using Estapar.Domain.ValueObjects;

namespace Estapar.Application.UseCases.Entry;

/// <summary>
/// Handles an ENTRY event: checks the sector isn't full, locks in the
/// dynamic price factor for the sector's occupancy at this moment, and
/// opens a new parking session.
/// </summary>
public sealed class HandleEntryUseCase
{
    private readonly ISectorRepository _sectors;
    private readonly IParkingSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public HandleEntryUseCase(ISectorRepository sectors, IParkingSessionRepository sessions, IUnitOfWork unitOfWork)
    {
        _sectors = sectors;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EntryCommand command, CancellationToken cancellationToken = default)
    {
        var sector = await _sectors.GetByNameAsync(command.Sector, cancellationToken)
            ?? throw new InvalidOperationException($"Unknown sector '{command.Sector}'.");

        var activeCount = await _sessions.CountActiveBySectorAsync(command.Sector, cancellationToken);

        if (sector.IsFull(activeCount))
            throw new GarageFullException(command.Sector);

        var priceFactor = sector.PriceFactorFor(activeCount);
        var licensePlate = LicensePlate.Create(command.LicensePlate);
        var session = ParkingSession.Open(licensePlate, command.Sector, command.EntryTime, priceFactor);

        await _sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
