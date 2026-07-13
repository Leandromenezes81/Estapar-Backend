using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Exceptions;

namespace Estapar.ParkingManager.Application.UseCases.Parked;

/// <summary>
/// Trata um evento PARKED: resolve a vaga física a partir de lat/lng,
/// marca-a como ocupada e a vincula à sessão em aberto do veículo. O fator
/// de preço já foi travado na ENTRY.
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

    /// <summary>Processa o evento PARKED informado, lançando <see cref="Estapar.ParkingManager.Domain.Exceptions.SessionNotFoundException"/> se não houver sessão em aberto para a placa.</summary>
    public async Task HandleAsync(ParkedCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetOpenByLicensePlateAsync(command.LicensePlate, cancellationToken)
            ?? throw new SessionNotFoundException(command.LicensePlate);

        var spot = await _spots.GetByCoordinatesAsync(command.SectorId, command.Lat, command.Lng, cancellationToken)
            ?? throw new InvalidOperationException($"Nenhuma vaga encontrada no setor '{command.SectorId}' nas coordenadas ({command.Lat}, {command.Lng}).");

        spot.Occupy();
        session.AssignSpot(spot.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
