using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.Exceptions;
using Estapar.ParkingManager.Domain.ValueObjects;

namespace Estapar.ParkingManager.Application.UseCases.Entry;

/// <summary>
/// Trata um evento ENTRY: verifica se o setor não está cheio, trava o fator
/// de preço dinâmico conforme a ocupação do setor no momento e abre uma
/// nova sessão de estacionamento.
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

    /// <summary>Processa o evento ENTRY informado, lançando <see cref="Estapar.ParkingManager.Domain.Exceptions.GarageFullException"/> se o setor estiver cheio.</summary>
    public async Task HandleAsync(EntryCommand command, CancellationToken cancellationToken = default)
    {
        var sector = await _sectors.GetByIdAsync(command.SectorId, cancellationToken)
            ?? throw new InvalidOperationException($"Setor de Id '{command.SectorId}' desconhecido.");

        var activeCount = await _sessions.CountActiveBySectorAsync(sector.Id, cancellationToken);

        if (sector.IsFull(activeCount))
            throw new GarageFullException(sector.Name);

        var priceFactor = sector.PriceFactorFor(activeCount);
        var licensePlate = LicensePlate.Create(command.LicensePlate);
        var session = ParkingSession.Open(licensePlate, sector.Id, command.EntryTime, priceFactor);

        await _sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
