using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Exceptions;

namespace Estapar.ParkingManager.Application.UseCases.Exit;

/// <summary>
/// Trata um evento EXIT: encerra a sessão (cobrando a tarifa com o fator de
/// preço travado e o preço base do setor) e libera a vaga.
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

    /// <summary>Processa o evento EXIT informado, lançando <see cref="Estapar.ParkingManager.Domain.Exceptions.SessionNotFoundException"/> se não houver sessão em aberto para a placa.</summary>
    public async Task HandleAsync(ExitCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetOpenByLicensePlateAsync(command.LicensePlate, cancellationToken)
            ?? throw new SessionNotFoundException(command.LicensePlate);

        var sector = await _sectors.GetByIdAsync(session.SectorId, cancellationToken)
            ?? throw new InvalidOperationException($"Setor de Id '{session.SectorId}' não encontrado.");

        session.Close(command.ExitTime, sector.BasePrice);

        if (session.SpotId is int spotId)
        {
            var spot = await _spots.GetByIdAsync(spotId, cancellationToken);
            spot?.Release();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
